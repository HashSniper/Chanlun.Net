"""数据同步逻辑

负责协调数据同步流程，包括：
- 单只股票的K线同步
- 全部A股股票的批量同步
- StockInfo 基本信息的同步
"""

import traceback
from datetime import datetime
from typing import List, Dict, Any, Optional

from config import FREQUENCY_MAP, BATCH_SIZE, DEFAULT_SYNC_START_DATE
from db_client import DbClient
from baostock_client import (
    BaostockClient,
    bs_code_to_symbol,
    bs_type_to_type,
    is_a_share_or_etf,
    is_a_share_stock,
    is_etf,
    is_t0_etf,
    parse_trade_time,
    symbol_to_bs_code,
)


# ============ StockInfo 同步 ============
def _upsert_stock_info_from_basic(
    db: DbClient,
    info: Dict[str, Any],
) -> bool:
    """
    根据 Baostock 返回的单条基本信息，写入/更新 StockInfo
    
    Args:
        db: 数据库客户端
        info: Baostock 返回的股票基本信息
    
    Returns:
        成功返回 True，失败返回 False
    """
    bs_code = info.get("code", "")
    if not bs_code:
        return False

    # 解析基本信息
    symbol = bs_code_to_symbol(bs_code)
    market = bs_code.split(".")[0].upper()
    name = info.get("code_name", "")

    # 判断证券类型
    type_ = bs_type_to_type(info.get("type", ""))
    if is_etf(bs_code):
        type_ = "etf"

    # 判断结算类型
    if type_ == "etf":
        settlement_type = "T0" if is_t0_etf(bs_code, name) else "T1"
    else:
        settlement_type = "T1"

    # 写入数据库
    ok = db.upsert_stock_info(symbol, name, market, type_, settlement_type)
    if ok:
        print(f"  [{symbol}] stock info upserted: {name} ({market}, {type_}, {settlement_type})")
    
    return ok


def sync_stock_info(
    db: DbClient,
    bs: BaostockClient,
    bs_code: str,
) -> bool:
    """
    同步单只股票基本信息到 StockInfo 表
    
    Args:
        db: 数据库客户端
        bs: Baostock 客户端
        bs_code: Baostock 格式的股票代码
    
    Returns:
        成功返回 True，失败返回 False
    """
    symbol = bs_code_to_symbol(bs_code)
    print(f"  [{symbol}] syncing stock info ...")

    try:
        info = bs.query_stock_basic(bs_code)
    except Exception as e:
        print(f"  [ERROR] query stock basic failed: {e}")
        return False

    if not info:
        print(f"  [WARN] no stock info found for {bs_code}")
        return False

    return _upsert_stock_info_from_basic(db, info)


# ============ K线数据同步 ============
def _fetch_and_parse_klines(
    bs: BaostockClient,
    bs_code: str,
    bs_freq: str,
    bs_start: str,
    bs_end: str,
) -> List[tuple]:
    """
    从 Baostock 获取K线数据并解析为数据库行格式
    
    Args:
        bs: Baostock 客户端
        bs_code: 股票代码
        bs_freq: Baostock 频率标识
        bs_start: 开始日期（YYYY-MM-DD）
        bs_end: 结束日期（YYYY-MM-DD）
    
    Returns:
        解析后的数据行列表，每行为 (Symbol, TradeTime, Open, High, Low, Close, Volume, Amount)
    """
    print(f"  [{bs_code_to_symbol(bs_code)}] fetching {bs_start} ~ {bs_end} ...")
    
    try:
        data = bs.query_history_k_data(bs_code, bs_start, bs_end, bs_freq)
    except Exception as e:
        print(f"  [ERROR] fetch failed: {e}")
        return []

    if not data:
        print(f"  [{bs_code_to_symbol(bs_code)}] no data")
        return []

    # 解析为数据库行格式
    rows = []
    for item in data:
        try:
            trade_time = parse_trade_time(item, bs_freq)
            rows.append((
                bs_code_to_symbol(bs_code),
                trade_time,
                float(item["open"]),
                float(item["high"]),
                float(item["low"]),
                float(item["close"]),
                float(item["volume"]) / 100,  # 成交量单位转换
                float(item["amount"]),
            ))
        except (ValueError, KeyError) as e:
            print(f"  [WARN] parse row failed: {item}, error: {e}")
            continue

    return rows


def _batch_insert_klines(
    db: DbClient,
    table_name: str,
    rows: List[tuple],
) -> int:
    """
    批量插入K线数据到数据库
    
    Args:
        db: 数据库客户端
        table_name: 表名
        rows: 数据行列表
    
    Returns:
        成功插入的行数
    """
    if not rows:
        return 0

    total_inserted = 0
    for i in range(0, len(rows), BATCH_SIZE):
        batch = rows[i : i + BATCH_SIZE]
        inserted = db.insert_klines(table_name, batch)
        total_inserted += inserted

    return total_inserted


def sync_stock(
    db: DbClient,
    bs: BaostockClient,
    bs_code: str,
    freq_key: str,
    start_date: Optional[datetime] = None,
    end_date: Optional[datetime] = None,
) -> int:
    """
    同步单只股票单周期的K线数据
    
    流程：
    1. 清空该股票在该表的历史数据
    2. 确定同步时间范围
    3. 从 Baostock 获取数据
    4. 解析并批量插入数据库
    
    Args:
        db: 数据库客户端
        bs: Baostock 客户端
        bs_code: Baostock 格式的股票代码
        freq_key: 周期标识（如 '5m', '30m', '1d'）
        start_date: 手动指定开始日期；None 则从 DEFAULT_SYNC_START_DATE 开始
        end_date: 手动指定结束日期；None 则为今天
    
    Returns:
        成功插入的行数
    """
    bs_freq, table_name = FREQUENCY_MAP[freq_key]
    symbol = bs_code_to_symbol(bs_code)

    # Step 1: 清空历史数据
    print(f"  [{symbol} {freq_key}] clearing existing data ...")
    deleted = db.clear_symbol_data(table_name, symbol)
    print(f"  [{symbol} {freq_key}] cleared {deleted} rows")

    # Step 2: 确定同步时间范围
    sync_start = start_date if start_date else DEFAULT_SYNC_START_DATE
    sync_end = end_date or datetime.now()

    if sync_start.date() > sync_end.date():
        print(f"  [{symbol} {freq_key}] already up-to-date")
        return 0

    bs_start = sync_start.strftime("%Y-%m-%d")
    bs_end = sync_end.strftime("%Y-%m-%d")

    # Step 3: 获取并解析数据
    rows = _fetch_and_parse_klines(bs, bs_code, bs_freq, bs_start, bs_end)
    if not rows:
        print(f"  [{symbol} {freq_key}] no valid rows")
        return 0

    # Step 4: 批量插入数据库
    total_inserted = _batch_insert_klines(db, table_name, rows)
    print(f"  [{symbol} {freq_key}] total inserted: {total_inserted}/{len(rows)} rows")
    
    return total_inserted


# ============ 单只股票完整同步 ============
def _sync_one_stock(
    db: DbClient,
    bs: BaostockClient,
    bs_code: str,
    frequencies: List[str],
    start_date: Optional[datetime],
    end_date: Optional[datetime],
) -> None:
    """
    同步单只股票的信息及其所有指定周期 K 线
    
    Args:
        db: 数据库客户端
        bs: Baostock 客户端
        bs_code: Baostock 格式的股票代码
        frequencies: 周期标识列表
        start_date: 开始日期
        end_date: 结束日期
    """
    # 同步股票基本信息
    try:
        sync_stock_info(db, bs, bs_code)
    except Exception as e:
        print(f"[ERROR] sync stock info {bs_code} failed: {e}")
        traceback.print_exc()

    # 同步各周期K线数据
    for freq in frequencies:
        try:
            sync_stock(db, bs, bs_code, freq, start_date, end_date)
        except Exception as e:
            print(f"[ERROR] sync {bs_code} {freq} failed: {e}")
            traceback.print_exc()


# ============ 批量同步接口 ============
def sync_all(
    stocks: List[str],
    frequencies: List[str] = None,
    start_date: Optional[datetime] = None,
    end_date: Optional[datetime] = None,
) -> None:
    """
    批量同步多只股票多周期
    
    每只股票每个周期同步前先清空该股票数据
    
    Args:
        stocks: Baostock 格式的股票代码列表
        frequencies: 周期标识列表，为 None 则同步所有周期
        start_date: 开始日期
        end_date: 结束日期
    """
    frequencies = frequencies or list(FREQUENCY_MAP.keys())

    with DbClient() as db, BaostockClient() as bs:
        for bs_code in stocks:
            _sync_one_stock(db, bs, bs_code, frequencies, start_date, end_date)


# ============ 全部A股同步 ============
def _load_a_share_list(
    bs: BaostockClient,
) -> List[Dict[str, Any]]:
    """
    一次性拉取全部 A 股股票和 ETF 的基本信息列表
    
    Returns:
        符合条件的股票基本信息列表
    """
    print("[FETCH] loading all A-share stock & ETF list ...")
    basics = bs.query_all_stock_basics()
    
    # 过滤出 A 股股票和 ETF
    items = [
        item
        for item in basics
        if is_a_share_or_etf(item["code"], item.get("type", ""))
    ]
    
    print(f"[FETCH] found {len(items)} A-share stocks & ETFs")
    return items


def sync_all_a_shares(
    frequencies: List[str] = None,
    start_date: Optional[datetime] = None,
    end_date: Optional[datetime] = None,
) -> None:
    """
    同步全部 A 股股票和 ETF 的 K 线数据
    
    从 StockInfo 表中原子认领 SyncStatus=0 的股票：
    - 认领成功后置为 1
    - 同步 K 线
    - 完成后置为 2
    
    支持多个进程/窗口同时运行，通过数据库原子操作避免重复处理同一只股票。
    
    Args:
        frequencies: 周期标识列表，为 None 则同步所有周期
        start_date: 开始日期
        end_date: 结束日期
    """
    frequencies = frequencies or list(FREQUENCY_MAP.keys())

    with DbClient() as db, BaostockClient() as bs:
        print("[START] syncing klines, claiming stocks from StockInfo ...")

        while True:
            # 原子认领一只股票
            symbol = db.claim_next_sync_stock()
            if symbol is None:
                print("[DONE] no more SyncStatus=0 stocks")
                break

            bs_code = symbol_to_bs_code(symbol)
            print(f"  [{symbol}] claimed, start syncing klines ...")

            # 同步该股票的所有周期K线
            all_success = True
            for freq in frequencies:
                try:
                    sync_stock(db, bs, bs_code, freq, start_date, end_date)
                except Exception as e:
                    all_success = False
                    print(f"[ERROR] sync {bs_code} {freq} failed: {e}")
                    traceback.print_exc()

            # 根据结果更新同步状态
            if all_success:
                db.update_stock_info_sync_status(symbol, 2)
                print(f"  [{symbol}] all frequencies synced, SyncStatus set to 2")
            else:
                db.update_stock_info_sync_status(symbol, 0)
                print(f"  [{symbol}] some frequencies failed, SyncStatus reset to 0")


def sync_all_stock_info() -> None:
    """
    仅同步全部 A 股股票和 ETF 的 StockInfo 基本信息
    
    只调用一次 Baostock 接口，获取全部证券列表后逐条写入数据库。
    适合在首次运行或需要全量更新基本信息时调用。
    """
    with DbClient() as db, BaostockClient() as bs:
        # 加载 A 股列表
        items = _load_a_share_list(bs)

        # 逐条写入数据库
        for info in items:
            try:
                _upsert_stock_info_from_basic(db, info)
            except Exception as e:
                print(f"[ERROR] sync stock info {info.get('code', '')} failed: {e}")
                traceback.print_exc()
