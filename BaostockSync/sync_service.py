"""数据同步逻辑"""

from datetime import datetime, timedelta
from typing import List

from config import FREQUENCY_MAP, BATCH_SIZE
from db_client import DbClient
from baostock_client import BaostockClient, bs_code_to_symbol, parse_trade_time


def batch_date_ranges(start_date: datetime, end_date: datetime, days: int):
    """将日期范围切分为多个批次"""
    current = start_date
    while current <= end_date:
        batch_end = min(current + timedelta(days=days - 1), end_date)
        yield current, batch_end
        current = batch_end + timedelta(days=1)


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
    start_date: 手动指定开始日期；None 则从数据库最新时间开始
    end_date:   手动指定结束日期；None 则为今天
    """
    from config import SYNC_BATCH_DAYS
    bs_freq, table_name = FREQUENCY_MAP[freq_key]
    symbol = bs_code_to_symbol(bs_code)

    # 确定同步时间范围
    db_max_time = db.get_max_trade_time(table_name, symbol)
    if start_date:
        sync_start = start_date
    elif db_max_time:
        sync_start = db_max_time + timedelta(days=1)
        # 如果是分钟线，从第二天开始；如果是日线，也从第二天开始
        if freq_key != "1d":
            sync_start = db_max_time + timedelta(minutes=1)
            sync_start = sync_start.replace(second=0, microsecond=0)
    else:
        sync_start = datetime(2020, 1, 1)

    sync_end = end_date or datetime.now()

    if sync_start.date() > sync_end.date():
        print(f"  [{symbol} {freq_key}] already up-to-date")
        return 0

    batch_days = SYNC_BATCH_DAYS.get(freq_key, 365)
    total_inserted = 0
    for batch_start, batch_end in batch_date_ranges(sync_start, sync_end, batch_days):
        bs_start = batch_start.strftime("%Y-%m-%d")
        bs_end = batch_end.strftime("%Y-%m-%d")

        print(f"  [{symbol} {freq_key}] fetching {bs_start} ~ {bs_end} ...")
        try:
            data = bs.query_history_k_data(bs_code, bs_start, bs_end, bs_freq)
        except Exception as e:
            print(f"  [ERROR] fetch failed: {e}")
            continue

        if not data:
            continue

        # 组装数据库行
        rows = []
        for item in data:
            try:
                trade_time = parse_trade_time(item, bs_freq)
                # 跳过已存在的数据（根据 db_max_time）
                if db_max_time and trade_time <= db_max_time:
                    continue
                rows.append((
                    symbol,
                    trade_time,
                    float(item["open"]),
                    float(item["high"]),
                    float(item["low"]),
                    float(item["close"]),
                    float(item["volume"]),
                    float(item["amount"]),
                ))
            except (ValueError, KeyError) as e:
                print(f"  [WARN] parse row failed: {item}, error: {e}")
                continue

        if not rows:
            continue

        # 批量插入
        inserted = 0
        for i in range(0, len(rows), BATCH_SIZE):
            batch = rows[i : i + BATCH_SIZE]
            inserted += db.insert_klines(table_name, batch)

        total_inserted += inserted
        print(f"  [{symbol} {freq_key}] inserted {inserted}/{len(rows)} rows")

    print(f"  [{symbol} {freq_key}] total inserted: {total_inserted}")
    return total_inserted


def sync_all(
    stocks: List[str],
    frequencies: List[str] = None,
    start_date: datetime | None = None,
    end_date: datetime | None = None,
):
    """批量同步多只股票多周期"""
    frequencies = frequencies or list(FREQUENCY_MAP.keys())

    with DbClient() as db, BaostockClient() as bs:
        for bs_code in stocks:
            for freq in frequencies:
                try:
                    sync_stock(db, bs, bs_code, freq, start_date, end_date)
                except Exception as e:
                    print(f"[ERROR] sync {bs_code} {freq} failed: {e}")
