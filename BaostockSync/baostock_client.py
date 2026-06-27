"""Baostock API 封装

封装与 Baostock 的所有交互，包括：
- 登录/登出管理
- K线数据查询
- 股票基本信息查询
- 数据格式转换（Baostock 代码 <-> 数据库 Symbol）
"""

import threading
import time
from datetime import datetime, timedelta
from typing import List, Dict, Any, Optional

import baostock as bs

from config import (
    BS_MAX_CONCURRENT_REQUESTS,
    BS_MAX_RETRIES,
    BS_RETRY_DELAYS,
    BS_ERROR_CODE_SUCCESS,
    T0_ETF_PREFIXES,
    T0_ETF_KEYWORDS,
    T0_ETF_CODES,
    T1_ETF_CODES,
    SHANGHAI_CODE_PREFIXES,
    ETF_CODE_PREFIXES,
)


# ============ 并发控制 ============
# 限制同时访问 Baostock API 的并发数，降低网络错误概率
_BS_API_SEMAPHORE = threading.Semaphore(BS_MAX_CONCURRENT_REQUESTS)


# ============ 错误处理 ============
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
) -> Any:
    """
    带退避重试的函数调用
    
    Args:
        func: 要调用的函数（无参数）
        max_retries: 最大重试次数
        delays: 每次重试的延迟时间（秒）
    
    Returns:
        函数执行的返回值
    
    Raises:
        最后一次尝试的异常
    """
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


# ============ 核心客户端类 ============
class BaostockClient:
    """Baostock API 客户端，支持上下文管理器（with 语句）"""
    
    def __init__(self):
        self._logged_in = False

    def login(self) -> None:
        """登录 Baostock，失败则抛出异常"""
        result = bs.login()
        if result.error_code != BS_ERROR_CODE_SUCCESS:
            print(f'[Baostock] login failed: error_code={result.error_code}, error_msg={result.error_msg}')
            raise RuntimeError(f"Baostock login failed (code={result.error_code})")
        self._logged_in = True
        print("[Baostock] login success")

    def logout(self) -> None:
        """登出 Baostock"""
        if self._logged_in:
            bs.logout()
            self._logged_in = False
            print("[Baostock] logout")

    def __enter__(self):
        """上下文管理器：进入时登录"""
        self.login()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器：退出时登出"""
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
        
        Args:
            code: 股票代码（如 sh.600000）
            start_date: 开始日期（YYYY-MM-DD）
            end_date: 结束日期（YYYY-MM-DD）
            frequency: 周期类型，'d'日线, '5'5分钟, '30'30分钟
        
        Returns:
            K线数据列表，每个元素为字典
        """
        # 日线不需要 time 字段，分钟线需要
        fields = "date,code,open,high,low,close,volume,amount"
        if frequency != "d":
            fields = "date,time,code,open,high,low,close,volume,amount"

        # 使用信号量控制并发，带重试机制
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

        # 检查查询是否成功
        if rs.error_code != BS_ERROR_CODE_SUCCESS:
            print(f'[Baostock] query failed: error_code={rs.error_code}, error_msg={rs.error_msg}')
            raise RuntimeError(f"Query failed for {code} [{frequency}]")

        # 解析返回数据
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

    def query_stock_basic(self, bs_code: str) -> Optional[Dict[str, Any]]:
        """
        查询单只股票基本信息
        
        Args:
            bs_code: Baostock 格式的股票代码（如 sh.600000）
        
        Returns:
            股票基本信息字典，未找到则返回 None
            字段: {"code", "code_name", "ipoDate", "outDate", "type", "status"}
        """
        with _BS_API_SEMAPHORE:
            rs = _call_with_retry(lambda: bs.query_stock_basic(code=bs_code))

        if rs.error_code != BS_ERROR_CODE_SUCCESS:
            print(f'[Baostock] query_stock_basic failed: error_code={rs.error_code}, error_msg={rs.error_msg}')
            raise RuntimeError(f"Query stock basic failed for {bs_code}")

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
        
        Returns:
            股票基本信息字典列表
            每个元素字段: {"code", "code_name", "ipoDate", "outDate", "type", "status"}
        """
        with _BS_API_SEMAPHORE:
            rs = _call_with_retry(lambda: bs.query_stock_basic())

        if rs.error_code != BS_ERROR_CODE_SUCCESS:
            print(f'[Baostock] query_all_stock_basics failed: error_code={rs.error_code}, error_msg={rs.error_msg}')
            raise RuntimeError(f"Query all stock basics failed")

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


# ============ 代码转换函数 ============
def bs_code_to_symbol(bs_code: str) -> str:
    """
    将 Baostock 代码转换为纯股票代码
    Example: sh.600000 -> 600000
    """
    return bs_code.split(".")[1]


def symbol_to_bs_code(symbol: str) -> str:
    """
    将纯股票代码转换为 Baostock 格式
    Example: 600000 -> sh.600000
    """
    code = symbol
    if code.startswith(SHANGHAI_CODE_PREFIXES):
        market = "sh"
    else:
        market = "sz"
    return f"{market}.{code}"


# ============ 时间解析 ============
def parse_trade_time(item: Dict[str, Any], frequency: str) -> datetime:
    """
    将 Baostock 返回的 date/time 解析为 datetime
    
    Args:
        item: K线数据字典
        frequency: 周期类型（'d', '5', '30'）
    """
    if frequency == "d":
        return datetime.strptime(item["date"], "%Y-%m-%d")
    else:
        # time 格式: YYYYMMDDhhmmssmmm 如 20240102093500000
        time_str = item["time"][:12]  # 取到分钟: YYYYMMDDhhmm
        return datetime.strptime(time_str, "%Y%m%d%H%M")


# ============ 证券类型判断 ============
def bs_type_to_type(bs_type: str) -> str:
    """
    把 Baostock 证券类型映射为 StockInfo.Type
    
    Baostock type:
        1: 股票
        2: 指数
    """
    mapping = {
        "1": "stock",
        "2": "index",
    }
    return mapping.get(bs_type, "other")


def is_etf(bs_code: str) -> bool:
    """根据代码前缀判断是否为 ETF"""
    code = bs_code_to_symbol(bs_code)
    return any(code.startswith(p) for p in ETF_CODE_PREFIXES)


def is_t0_etf(bs_code: str, name: str = "") -> bool:
    """
    判断 ETF 是否支持 T+0 交易。
    
    优先级（从高到低）：
    1. T1_ETF_CODES 黑名单
    2. T0_ETF_CODES 白名单
    3. T0_ETF_PREFIXES 代码前缀
    4. T0_ETF_KEYWORDS 名称关键字
    """
    code = bs_code_to_symbol(bs_code)
    name = name or ""

    # 黑名单优先
    if code in T1_ETF_CODES:
        return False
    
    # 白名单
    if code in T0_ETF_CODES:
        return True

    # 代码前缀
    if any(code.startswith(p) for p in T0_ETF_PREFIXES):
        return True

    # 名称关键字
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
    """判断是否属于 A 股股票（排除指数和 ETF）"""
    if bs_type == "2":
        return False

    if is_etf(bs_code):
        return False

    if bs_type == "1":
        return True

    return False
