"""SQL Server 数据库操作"""

import pyodbc
from datetime import datetime
from typing import List, Tuple
from config import DB_CONN_STR


class DbClient:
    def __init__(self):
        self.conn = pyodbc.connect(DB_CONN_STR)

    def close(self):
        if self.conn:
            self.conn.close()

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        self.close()

    def get_max_trade_time(self, table: str, symbol: str) -> datetime | None:
        """获取某只股票在某表中的最新交易时间，用于增量同步"""
        sql = f"SELECT MAX(TradeTime) FROM {table} WHERE Symbol = ?"
        cursor = self.conn.cursor()
        cursor.execute(sql, (symbol,))
        row = cursor.fetchone()
        cursor.close()
        return row[0] if row and row[0] else None

    def insert_klines(self, table: str, rows: List[Tuple]) -> int:
        """
        批量插入K线数据
        rows: [(Symbol, TradeTime, Open, High, Low, Close, Volume, Amount), ...]
        """
        if not rows:
            return 0

        sql = f"""
            INSERT INTO {table} (Symbol, TradeTime, [Open], High, Low, [Close], Volume, Amount, CreatedAt)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        """
        now = datetime.now()
        # 添加 CreatedAt
        rows_with_time = [(*r, now) for r in rows]

        cursor = self.conn.cursor()
        inserted = 0
        try:
            cursor.executemany(sql, rows_with_time)
            self.conn.commit()
            # pyodbc executemany 对 SQL Server 可能返回 -1，用 len 兜底
            inserted = len(rows_with_time) if cursor.rowcount == -1 else cursor.rowcount
        except pyodbc.Error:
            self.conn.rollback()
            # 处理重复键冲突时逐条插入
            for row in rows_with_time:
                try:
                    cursor.execute(sql, row)
                    inserted += 1
                except pyodbc.IntegrityError:
                    pass  # 重复数据跳过
            self.conn.commit()
        finally:
            cursor.close()
        return inserted
