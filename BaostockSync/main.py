"""
Baostock 历史行情数据同步工具

用法:
    python main.py --stock sh.600000 --freq 5m --start 2024-01-01 --end 2024-06-01
    python main.py --stock sh.600000 sh.600519 sz.000001 --freq 5m 30m 1d
    python main.py --all --freq 1d
"""

import argparse
from datetime import datetime

from config import DEFAULT_STOCKS, FREQUENCY_MAP
from sync_service import sync_all


def main():
    parser = argparse.ArgumentParser(description="同步 Baostock 历史K线到 SQL Server")
    parser.add_argument(
        "--stock",
        nargs="+",
        help="Baostock 股票代码，如 sh.600000 sz.000001",
    )
    parser.add_argument(
        "--freq",
        nargs="+",
        choices=list(FREQUENCY_MAP.keys()),
        default=list(FREQUENCY_MAP.keys()),
        help="同步周期: 5m/30m/1d",
    )
    parser.add_argument(
        "--start",
        type=str,
        help="开始日期，如 2024-01-01",
    )
    parser.add_argument(
        "--end",
        type=str,
        help="结束日期，如 2024-06-01",
    )
    parser.add_argument(
        "--all",
        action="store_true",
        help="同步默认股票列表",
    )

    args = parser.parse_args()

    stocks = args.stock or (DEFAULT_STOCKS if args.all else [])
    if not stocks:
        parser.print_help()
        return

    start = datetime.strptime(args.start, "%Y-%m-%d") if args.start else None
    end = datetime.strptime(args.end, "%Y-%m-%d") if args.end else None

    print("=" * 50)
    print(f"Stocks : {stocks}")
    print(f"Freqs  : {args.freq}")
    print(f"Start  : {start}")
    print(f"End    : {end}")
    print("=" * 50)

    sync_all(stocks, args.freq, start, end)
    print("Done.")


if __name__ == "__main__":
    main()
