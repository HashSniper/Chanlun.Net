"""SQL Server 数据库操作

封装所有数据库操作，包括：
- 连接管理（支持上下文管理器）
- K线数据批量插入
- StockInfo 表的增删改查
- 并发安全的股票认领机制
"""

import pyodbc
from datetime import datetime
from typing import List, Tuple, Optional


class DbClient:
    """
    数据库客户端，支持上下文管理器（with 语句）
    
    Usage:
        with DbClient() as db:
            db.insert_klines(...)
    """
    
    def __init__(self, conn_str: str = None):
        """
        初始化数据库连接
        
        Args:
            conn_str: 数据库连接字符串，为 None 则使用 config 中的配置
        """
        from config import DB_CONN_STR
        self.conn = pyodbc.connect(conn_str or DB_CONN_STR)

    def close(self) -> None:
        """关闭数据库连接"""
        if self.conn:
            self.conn.close()

    def __enter__(self):
        """上下文管理器：进入时返回自身"""
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器：退出时关闭连接"""
        self.close()

    # ============ StockInfo 表操作 ============
    def upsert_stock_info(
        self,
        symbol: str,
        name: str,
        exchange: str,
        type_: str,
        settlement_type: str = "T1",
    ) -> bool:
        """
        插入或更新 StockInfo 表（UPSERT）
        
        Args:
            symbol: 股票代码（纯数字，如 600000）
            name: 股票名称
            exchange: 交易所（sh 或 sz）
            type_: 类型（stock/etf/index/other）
            settlement_type: 结算类型（T+0 或 T+1）
        
        Returns:
            成功返回 True，失败返回 False
        """
        now = datetime.now()
        sql = """
            MERGE StockInfo AS target
            USING (VALUES (?, ?, ?, ?, ?)) AS source (Symbol, Name, Exchange, Type, SettlementType)
            ON target.Symbol = source.Symbol
            WHEN MATCHED THEN
                UPDATE SET Name = source.Name,
                           Exchange = source.Exchange,
                           Type = source.Type,
                           SettlementType = source.SettlementType,
                           UpdatedAt = ?
            WHEN NOT MATCHED THEN
                INSERT (Symbol, Name, Exchange, Type, SettlementType, CreatedAt, UpdatedAt)
                VALUES (source.Symbol, source.Name, source.Exchange, source.Type, source.SettlementType, ?, ?);
        """
        
        cursor = self.conn.cursor()
        try:
            cursor.execute(
                sql,
                (symbol, name, exchange, type_, settlement_type, now, now, now),
            )
            self.conn.commit()
            return True
        except pyodbc.Error as e:
            self.conn.rollback()
            print(f"  [ERROR] upsert StockInfo failed: {e}")
            return False
        finally:
            cursor.close()

    def get_all_stock_symbols(self) -> List[str]:
        """
        获取 StockInfo 表中所有股票的 Symbol
        
        Returns:
            股票代码列表，查询失败返回空列表
        """
        sql = "SELECT Symbol FROM StockInfo ORDER BY Symbol"
        cursor = self.conn.cursor()
        try:
            cursor.execute(sql)
            rows = cursor.fetchall()
            return [row[0] for row in rows]
        except pyodbc.Error as e:
            print(f"  [ERROR] get all stock symbols failed: {e}")
            return []
        finally:
            cursor.close()

    def get_stock_info_sync_status(self, symbol: str) -> Optional[int]:
        """
        获取某只股票的 SyncStatus
        
        Args:
            symbol: 股票代码
        
        Returns:
            同步状态（0/1/2），StockInfo 中不存在时返回 None
        """
        sql = "SELECT SyncStatus FROM StockInfo WHERE Symbol = ?"
        cursor = self.conn.cursor()
        try:
            cursor.execute(sql, (symbol,))
            row = cursor.fetchone()
            return row[0] if row else None
        except pyodbc.Error as e:
            print(f"  [ERROR] get sync status failed: {e}")
            return None
        finally:
            cursor.close()

    def update_stock_info_sync_status(self, symbol: str, status: int) -> bool:
        """
        更新某只股票的 SyncStatus
        
        Args:
            symbol: 股票代码
            status: 新状态值
        
        Returns:
            更新成功返回 True，否则返回 False
        """
        sql = """
            UPDATE StockInfo
            SET SyncStatus = ?, UpdatedAt = ?
            WHERE Symbol = ?
        """
        cursor = self.conn.cursor()
        try:
            cursor.execute(sql, (status, datetime.now(), symbol))
            self.conn.commit()
            return cursor.rowcount > 0
        except pyodbc.Error as e:
            self.conn.rollback()
            print(f"  [ERROR] update sync status failed: {e}")
            return False
        finally:
            cursor.close()

    # ============ 并发安全的股票认领 ============
    def claim_next_sync_stock(self) -> Optional[str]:
        """
        原子地从 StockInfo 中认领下一只 SyncStatus=0 的股票，并将其置为 1。
        
        使用 READPAST 跳过已被其他会话锁定的行，适合多进程并发同步。
        
        Returns:
            认领的股票代码（Symbol），没有则返回 None
        """
        sql = """
            UPDATE TOP (1) StockInfo WITH (READPAST)
            SET SyncStatus = 1, UpdatedAt = ?
            OUTPUT inserted.Symbol
            WHERE SyncStatus = 0 AND Type = 'stock'
        """
        cursor = self.conn.cursor()
        try:
            cursor.execute(sql, (datetime.now(),))
            row = cursor.fetchone()
            self.conn.commit()
            return row[0] if row else None
        except pyodbc.Error as e:
            self.conn.rollback()
            print(f"  [ERROR] claim next sync stock failed: {e}")
            return None
        finally:
            cursor.close()

    # ============ K线数据操作 ============
    def get_max_trade_time(self, table: str, symbol: str) -> Optional[datetime]:
        """
        获取某只股票在某表中的最新交易时间，用于增量同步
        
        Args:
            table: K线表名（如 Kline_1d）
            symbol: 股票代码
        
        Returns:
            最新交易时间，表中无数据时返回 None
        """
        sql = f"SELECT MAX(TradeTime) FROM {table} WHERE Symbol = ?"
        cursor = self.conn.cursor()
        cursor.execute(sql, (symbol,))
        row = cursor.fetchone()
        cursor.close()
        return row[0] if row and row[0] else None

    def clear_symbol_data(self, table: str, symbol: str) -> int:
        """
        清空某只股票在指定表中的数据
        
        Args:
            table: K线表名
            symbol: 股票代码
        
        Returns:
            删除的行数
        """
        cursor = self.conn.cursor()
        try:
            cursor.execute(f"DELETE FROM {table} WHERE Symbol = ?", (symbol,))
            self.conn.commit()
            deleted = cursor.rowcount
        except pyodbc.Error:
            self.conn.rollback()
            raise
        finally:
            cursor.close()
        return deleted

    def insert_klines(self, table: str, rows: List[Tuple]) -> int:
        """
        批量插入K线数据
        
        Args:
            table: K线表名
            rows: 数据行列表，每行为 (Symbol, TradeTime, Open, High, Low, Close, Volume, Amount)
        
        Returns:
            成功插入的行数
        """
        if not rows:
            return 0

        sql = f"""
            INSERT INTO {table} (Symbol, TradeTime, [Open], High, Low, [Close], Volume, Amount, CreatedAt)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        """
        now = datetime.now()
        # 添加 CreatedAt 时间戳
        rows_with_time = [(*r, now) for r in rows]

        cursor = self.conn.cursor()
        inserted = 0
        try:
            # 尝试批量插入
            cursor.executemany(sql, rows_with_time)
            self.conn.commit()
            # pyodbc executemany 对 SQL Server 可能返回 -1，用 len 兜底
            inserted = len(rows_with_time) if cursor.rowcount == -1 else cursor.rowcount
        except pyodbc.Error:
            # 批量插入失败，逐条插入（处理重复键冲突）
            self.conn.rollback()
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
