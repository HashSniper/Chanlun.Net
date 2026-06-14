"""数据同步逻辑"""

import traceback
from datetime import datetime
from typing import List, Dict, Any

from config import FREQUENCY_MAP, BATCH_SIZE
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


def _upsert_stock_info_from_basic(
    db: DbClient,
    info: Dict[str, Any],
) -> bool:
    """根据 Baostock query_stock_basic 返回的单条数据，写入/更新 StockInfo"""
    bs_code = info.get("code", "")
    if not bs_code:
        return False

    symbol = bs_code_to_symbol(bs_code)
    market = bs_code.split(".")[0].upper()
    name = info.get("code_name", "")

    type_ = bs_type_to_type(info.get("type", ""))
    if is_etf(bs_code):
        type_ = "etf"

    # 结算类型：股票 T+1；ETF 根据代码段和名称判断是否为 T+0，否则 T+1
    if type_ == "etf":
        settlement_type = "T0" if is_t0_etf(bs_code, name) else "T1"
    else:
        settlement_type = "T1"

    ok = db.upsert_stock_info(symbol, name, market, type_, settlement_type)
    if ok:
        print(f"  [{symbol}] stock info upserted: {name} ({market}, {type_}, {settlement_type})")
    return ok


def sync_stock_info(
    db: DbClient,
    bs: BaostockClient,
    bs_code: str,
) -> bool:
    """同步单只股票基本信息到 StockInfo 表"""
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


def sync_stock(
    db: DbClient,
    bs: BaostockClient,
    bs_code: str,
    freq_key: str,
    start_date: datetime | None = None,
    end_date: datetime | None = None,
) -> int:
    """
    同步单只股票单周期的K线数据
    start_date: 手动指定开始日期；None 则从 2020-01-01 开始
    end_date:   手动指定结束日期；None 则为今天
    """
    bs_freq, table_name = FREQUENCY_MAP[freq_key]
    symbol = bs_code_to_symbol(bs_code)

    # 同步前清空该股票在该表中的历史数据
    print(f"  [{symbol} {freq_key}] clearing existing data ...")
    deleted = db.clear_symbol_data(table_name, symbol)
    print(f"  [{symbol} {freq_key}] cleared {deleted} rows")

    # 确定同步时间范围：未指定起始时间则统一从 2020-01-01 开始
    sync_start = start_date if start_date else datetime(2020, 1, 1)
    sync_end = end_date or datetime.now()

    if sync_start.date() > sync_end.date():
        print(f"  [{symbol} {freq_key}] already up-to-date")
        return 0

    bs_start = sync_start.strftime("%Y-%m-%d")
    bs_end = sync_end.strftime("%Y-%m-%d")

    print(f"  [{symbol} {freq_key}] fetching {bs_start} ~ {bs_end} ...")
    try:
        data = bs.query_history_k_data(bs_code, bs_start, bs_end, bs_freq)
    except Exception as e:
        print(f"  [ERROR] fetch failed: {e}")
        return 0

    if not data:
        print(f"  [{symbol} {freq_key}] no data")
        return 0

    # 组装数据库行
    rows = []
    for item in data:
        try:
            trade_time = parse_trade_time(item, bs_freq)
            rows.append((
                symbol,
                trade_time,
                float(item["open"]),
                float(item["high"]),
                float(item["low"]),
                float(item["close"]),
                float(item["volume"]) / 100,
                float(item["amount"]),
            ))
        except (ValueError, KeyError) as e:
            print(f"  [WARN] parse row failed: {item}, error: {e}")
            continue

    if not rows:
        print(f"  [{symbol} {freq_key}] no valid rows")
        return 0

    # 批量插入
    total_inserted = 0
    for i in range(0, len(rows), BATCH_SIZE):
        batch = rows[i : i + BATCH_SIZE]
        total_inserted += db.insert_klines(table_name, batch)

    print(f"  [{symbol} {freq_key}] total inserted: {total_inserted}/{len(rows)} rows")
    return total_inserted


def _sync_one_stock(
    db: DbClient,
    bs: BaostockClient,
    bs_code: str,
    frequencies: List[str],
    start_date: datetime | None,
    end_date: datetime | None,
):
    """同步单只股票的信息及其所有指定周期 K 线"""
    try:
        sync_stock_info(db, bs, bs_code)
    except Exception as e:
        print(f"[ERROR] sync stock info {bs_code} failed: {e}")
        traceback.print_exc()

    for freq in frequencies:
        try:
            sync_stock(db, bs, bs_code, freq, start_date, end_date)
        except Exception as e:
            print(f"[ERROR] sync {bs_code} {freq} failed: {e}")
            traceback.print_exc()


def sync_all(
    stocks: List[str],
    frequencies: List[str] = None,
    start_date: datetime | None = None,
    end_date: datetime | None = None,
):
    """批量同步多只股票多周期；每只股票每个周期同步前先清空该股票数据"""
    frequencies = frequencies or list(FREQUENCY_MAP.keys())

    with DbClient() as db, BaostockClient() as bs:
        for bs_code in stocks:
            _sync_one_stock(db, bs, bs_code, frequencies, start_date, end_date)


def _load_a_share_list(
    bs: BaostockClient,
) -> List[Dict[str, Any]]:
    """一次性拉取全部 A 股股票的基本信息列表（不含 ETF）"""
    print("[FETCH] loading all A-share stock list ...")
    basics = bs.query_all_stock_basics()
    items = [
        item
        for item in basics
        if is_a_share_stock(item["code"], item.get("type", ""))
    ]
    print(f"[FETCH] found {len(items)} A-share stocks")
    return items


def sync_all_a_shares(
    frequencies: List[str] = None,
    start_date: datetime | None = None,
    end_date: datetime | None = None,
):
    """
    同步全部 A 股股票和 ETF 的 K 线数据。
    从 StockInfo 表中原子认领 SyncStatus=0 的股票：
        - 认领成功后置为 1
        - 同步 K 线
        - 完成后置为 2
    支持多个进程/窗口同时运行，通过数据库原子操作避免重复处理同一只股票。
    """
    frequencies = frequencies or list(FREQUENCY_MAP.keys())

    with DbClient() as db, BaostockClient() as bs:
        print("[START] syncing klines, claiming stocks from StockInfo ...")

        while True:
            symbol = db.claim_next_sync_stock()
            if symbol is None:
                print("[DONE] no more SyncStatus=0 stocks")
                break

            bs_code = symbol_to_bs_code(symbol)
            print(f"  [{symbol}] claimed, start syncing klines ...")

            # 同步 K 线数据
            all_success = True
            for freq in frequencies:
                try:
                    sync_stock(db, bs, bs_code, freq, start_date, end_date)
                except Exception as e:
                    all_success = False
                    print(f"[ERROR] sync {bs_code} {freq} failed: {e}")
                    traceback.print_exc()

            # 全部成功则置为 2，任一失败则置回 0 以便重试
            if all_success:
                db.update_stock_info_sync_status(symbol, 2)
                print(f"  [{symbol}] all frequencies synced, SyncStatus set to 2")
            else:
                db.update_stock_info_sync_status(symbol, 0)
                print(f"  [{symbol}] some frequencies failed, SyncStatus reset to 0")


def sync_all_stock_info():
    """仅同步全部 A 股股票和 ETF 的 StockInfo 基本信息（只调用一次 Baostock 接口）"""
    with DbClient() as db, BaostockClient() as bs:
        items = _load_a_share_list(bs)

        for info in items:
            try:
                _upsert_stock_info_from_basic(db, info)
            except Exception as e:
                print(f"[ERROR] sync stock info {info.get('code', '')} failed: {e}")
                traceback.print_exc()
