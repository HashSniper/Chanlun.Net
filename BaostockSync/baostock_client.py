"""Baostock API 封装"""

import baostock as bs
from datetime import datetime, timedelta
from typing import List, Dict, Any


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

        rs = bs.query_history_k_data_plus(
            code,
            fields,
            start_date=start_date,
            end_date=end_date,
            frequency=frequency,
        )

        if rs.error_code != "0":
            raise RuntimeError(
                f"Query failed for {code} [{frequency}]: {rs.error_msg}"
            )

        data = []
        while rs.next():
            row = rs.get_row_data()
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
