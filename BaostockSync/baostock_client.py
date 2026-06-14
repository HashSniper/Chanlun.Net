"""Baostock API 封装"""

import threading
import time

import baostock as bs
from datetime import datetime, timedelta
from typing import List, Dict, Any

from config import (
    T0_ETF_PREFIXES,
    T0_ETF_KEYWORDS,
    T0_ETF_CODES,
    T1_ETF_CODES,
    BS_MAX_CONCURRENT_REQUESTS,
    BS_MAX_RETRIES,
    BS_RETRY_DELAYS,
)


# 限制同时访问 Baostock API 的并发数，降低网络错误概率
_BS_API_SEMAPHORE = threading.Semaphore(BS_MAX_CONCURRENT_REQUESTS)


def _is_retryable_error(exc: Exception) -> bool:
    """判断是否为可重试的网络/解码错误"""
    msg = str(exc).lower()
    retryable_keywords = [
        "网络接收错误",
        "codec",
        "decode",
        "utf-8",
        "connection",
        "timeout",
        "temporarily",
    ]
    return any(k in msg for k in retryable_keywords)


def _call_with_retry(
    func,
    max_retries: int = BS_MAX_RETRIES,
    delays: tuple = BS_RETRY_DELAYS,
):
    """带退避重试的函数调用"""
    last_exc = None
    for attempt in range(max_retries):
        try:
            return func()
        except Exception as e:
            last_exc = e
            if _is_retryable_error(e) and attempt < max_retries - 1:
                wait = delays[min(attempt, len(delays) - 1)]
                print(f"  [WARN] request failed ({e}), retry in {wait}s ({attempt + 1}/{max_retries}) ...")
                time.sleep(wait)
                continue
            raise
    raise last_exc


class BaostockClient:
    def __init__(self):
        self._logged_in = False

    def login(self):
        result = bs.login()
        if result.error_code != "0":
            raise RuntimeError(f"Baostock login failed: {result.error_msg}")
        self._logged_in = True
        print("[Baostock] login success")

    def logout(self):
        if self._logged_in:
            bs.logout()
            self._logged_in = False
            print("[Baostock] logout")

    def __enter__(self):
        self.login()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        self.logout()

    def query_history_k_data(
        self,
        code: str,
        start_date: str,
        end_date: str,
        frequency: str,
    ) -> List[Dict[str, Any]]:
        """
        查询历史K线数据
        frequency: 'd'日线, '5'5分钟, '30'30分钟
        """
        # 日线不需要 time 字段，分钟线需要
        fields = "date,code,open,high,low,close,volume,amount"
        if frequency != "d":
            fields = "date,time,code,open,high,low,close,volume,amount"

        with _BS_API_SEMAPHORE:
            rs = _call_with_retry(
                lambda: bs.query_history_k_data_plus(
                    code,
                    fields,
                    start_date=start_date,
                    end_date=end_date,
                    frequency=frequency,
                )
            )
        if rs.error_code != "0":
            raise RuntimeError(
                f"Query failed for {code} [{frequency}]: {rs.error_msg}"
            )

        data = []
        expected_cols = 8 if frequency == "d" else 9
        while rs.next():
            row = rs.get_row_data()
            if len(row) < expected_cols:
                print(f"  [WARN] skip malformed row from {code} [{frequency}]: {row}")
                continue
            # 根据字段顺序解析
            if frequency == "d":
                data.append({
                    "date": row[0],
                    "code": row[1],
                    "open": row[2],
                    "high": row[3],
                    "low": row[4],
                    "close": row[5],
                    "volume": row[6],
                    "amount": row[7],
                })
            else:
                data.append({
                    "date": row[0],
                    "time": row[1],
                    "code": row[2],
                    "open": row[3],
                    "high": row[4],
                    "low": row[5],
                    "close": row[6],
                    "volume": row[7],
                    "amount": row[8],
                })
        return data

    def query_stock_basic(self, bs_code: str) -> Dict[str, Any] | None:
        """
        查询股票基本信息
        返回: {"code", "code_name", "ipoDate", "outDate", "type", "status"}
        """
        with _BS_API_SEMAPHORE:
            rs = _call_with_retry(lambda: bs.query_stock_basic(code=bs_code))

        if rs.error_code != "0":
            raise RuntimeError(
                f"Query stock basic failed for {bs_code}: {rs.error_msg}"
            )

        while rs.next():
            row = rs.get_row_data()
            if len(row) < 6:
                print(f"  [WARN] skip malformed stock basic row: {row}")
                continue
            return {
                "code": row[0],
                "code_name": row[1],
                "ipoDate": row[2],
                "outDate": row[3],
                "type": row[4],
                "status": row[5],
            }
        return None

    def query_all_stock_basics(self) -> List[Dict[str, Any]]:
        """
        查询全部证券基本信息
        返回: [{"code", "code_name", "ipoDate", "outDate", "type", "status"}, ...]
        """
        with _BS_API_SEMAPHORE:
            rs = _call_with_retry(lambda: bs.query_stock_basic())

        if rs.error_code != "0":
            raise RuntimeError(
                f"Query all stock basics failed: {rs.error_msg}"
            )

        data = []
        while rs.next():
            row = rs.get_row_data()
            if len(row) < 6:
                print(f"  [WARN] skip malformed stock basic row: {row}")
                continue
            data.append({
                "code": row[0],
                "code_name": row[1],
                "ipoDate": row[2],
                "outDate": row[3],
                "type": row[4],
                "status": row[5],
            })
        return data


# Baostock code 与 DB Symbol 的互转
def bs_code_to_symbol(bs_code: str) -> str:
    """sh.600000 -> 600000 (纯代码，不带市场前缀)"""
    return bs_code.split(".")[1]


def symbol_to_bs_code(symbol: str) -> str:
    """600000 -> sh.600000 (根据代码前缀判断市场)"""
    code = symbol
    if code.startswith("6") or code.startswith("68") or code.startswith("88"):
        market = "sh"
    else:
        market = "sz"
    return f"{market}.{code}"


def parse_trade_time(item: Dict[str, Any], frequency: str) -> datetime:
    """将 Baostock 返回的 date/time 解析为 datetime"""
    if frequency == "d":
        return datetime.strptime(item["date"], "%Y-%m-%d")
    else:
        # time 格式: YYYYMMDDhhmmssmmm 如 20240102093500000
        time_str = item["time"][:12]  # 取到分钟: YYYYMMDDhhmm
        return datetime.strptime(time_str, "%Y%m%d%H%M")


def bs_type_to_type(bs_type: str) -> str:
    """把 Baostock 证券类型映射为 StockInfo.Type"""
    mapping = {
        "1": "stock",
        "2": "index",
    }
    return mapping.get(bs_type, "other")


def is_etf(bs_code: str) -> bool:
    """根据代码前缀判断是否为 ETF"""
    code = bs_code_to_symbol(bs_code)
    etf_prefixes = (
        "510", "511", "512", "513", "515", "516", "517", "518",
        "560", "561", "562", "563", "564", "565", "566", "567",
        "568", "569", "58",
        "159",
    )
    return any(code.startswith(p) for p in etf_prefixes)


def is_t0_etf(bs_code: str, name: str = "") -> bool:
    """
    判断 ETF 是否支持 T+0 交易。
    优先使用 config 中的白名单/黑名单，其次按代码前缀和名称关键字推断。
    """
    code = bs_code_to_symbol(bs_code)
    name = name or ""

    if code in T1_ETF_CODES:
        return False
    if code in T0_ETF_CODES:
        return True

    if any(code.startswith(p) for p in T0_ETF_PREFIXES):
        return True

    if any(k in name for k in T0_ETF_KEYWORDS):
        return True

    return False


def is_a_share_or_etf(bs_code: str, bs_type: str) -> bool:
    """
    判断是否属于 A 股股票或 ETF
    - 排除指数（type=2）
    - 保留 type=1 的股票（通常已包含 ETF）
    - 对 ETF 常见代码段做兜底识别
    """
    if bs_type == "2":
        return False

    if is_etf(bs_code):
        return True

    # type=1 默认认为是股票
    if bs_type == "1":
        return True

    return False


def is_a_share_stock(bs_code: str, bs_type: str) -> bool:
    """
    判断是否属于 A 股股票（排除指数和 ETF）
    """
    if bs_type == "2":
        return False

    if is_etf(bs_code):
        return False

    if bs_type == "1":
        return True

    return False
