"""
Baostock 历史行情数据同步工具

用法:
    # 同步指定股票的K线数据
    python main.py --stock sh.600000 --freq 5m --start 2024-01-01 --end 2024-06-01
    
    # 同步多只股票的多周期数据
    python main.py --stock sh.600000 sh.600519 sz.000001 --freq 5m 30m 1d
    
    # 同步默认股票列表
    python main.py --all --freq 1d
    
    # 同步全部A股（从 StockInfo 表认领）
    python main.py --all-a-shares --freq 1d
    
    # 仅同步股票基本信息
    python main.py --sync-stock-info
"""

import argparse
from datetime import datetime
from typing import List, Optional

from config import DEFAULT_STOCKS, FREQUENCY_MAP
from sync_service import sync_all, sync_all_a_shares, sync_all_stock_info
from baostock_client import symbol_to_bs_code


# ============ 股票代码处理 ============
def normalize_stock_code(code: str) -> str:
    """
    统一股票代码为 Baostock 格式（sh.xxxxxx / sz.xxxxxx）
    
    Args:
        code: 股票代码，支持格式：
              - Baostock 格式：sh.600000, sz.000001
              - 纯代码格式：600000, 000001
    
    Returns:
        Baostock 格式的股票代码
    """
    code = code.strip()
    if "." in code:
        return code  # 已经是 Baostock 格式
    return symbol_to_bs_code(code)


# ============ 参数验证 ============
def _print_sync_config(
    stocks: List[str],
    frequencies: List[str],
    start_date: Optional[datetime],
    end_date: Optional[datetime],
) -> None:
    """
    打印同步配置信息
    
    Args:
        stocks: 股票代码列表
        frequencies: 周期列表
        start_date: 开始日期
        end_date: 结束日期
    """
    print("=" * 50)
    print(f"Stocks : {stocks}")
    print(f"Freqs   : {frequencies}")
    print(f"Start   : {start_date}")
    print(f"End     : {end_date}")
    print("=" * 50)


def _parse_date(date_str: str) -> Optional[datetime]:
    """
    解析日期字符串为 datetime 对象
    
    Args:
        date_str: 日期字符串（YYYY-MM-DD 格式）
    
    Returns:
        datetime 对象，解析失败返回 None
    """
    try:
        return datetime.strptime(date_str, "%Y-%m-%d")
    except ValueError:
        print(f"[ERROR] Invalid date format: {date_str}, expected YYYY-MM-DD")
        return None


# ============ 主函数 ============
def main() -> None:
    """主函数：解析命令行参数并执行同步任务"""
    parser = argparse.ArgumentParser(
        description="同步 Baostock 历史K线到 SQL Server",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
示例:
  python main.py --stock sh.600000 --freq 5m --start 2024-01-01
  python main.py --all-a-shares --freq 1d
  python main.py --sync-stock-info
        """
    )
    
    # 股票代码参数
    parser.add_argument(
        "--stock",
        nargs="+",
        type=str,
        help="Baostock 股票代码，如 sh.600000 sz.000001 或 600000 000001",
    )
    
    # 周期参数
    parser.add_argument(
        "--freq",
        nargs="+",
        choices=list(FREQUENCY_MAP.keys()),
        default=list(FREQUENCY_MAP.keys()),
        help="同步周期: 5m/30m/1d",
    )
    
    # 日期参数
    parser.add_argument(
        "--start",
        type=str,
        help="开始日期，格式: YYYY-MM-DD",
    )
    parser.add_argument(
        "--end",
        type=str,
        help="结束日期，格式: YYYY-MM-DD",
    )
    
    # 同步模式参数
    parser.add_argument(
        "--all",
        action="store_true",
        help="同步默认股票列表",
    )
    parser.add_argument(
        "--all-a-shares",
        action="store_true",
        help="根据 StockInfo 表同步全部 A 股股票和 ETF 的 K 线（从 2021-01-01 到现在）",
    )
    parser.add_argument(
        "--sync-stock-info",
        action="store_true",
        help="仅同步全部 A 股股票和 ETF 的 StockInfo 基本信息",
    )

    args = parser.parse_args()

    # ---- 模式1：仅同步股票基本信息 ----
    if args.sync_stock_info:
        print("=" * 50)
        print("Mode   : sync all stock info only")
        print("=" * 50)
        sync_all_stock_info()
        print("Done.")
        return

    # ---- 模式2：同步全部A股 ----
    if args.all_a_shares:
        start = _parse_date(args.start) if args.start else None
        end = _parse_date(args.end) if args.end else None

        if args.start and start is None or args.end and end is None:
            return  # 日期格式错误，已打印错误信息

        _print_sync_config(["all A-shares"], args.freq, start, end)
        sync_all_a_shares(args.freq, start, end)
        print("Done.")
        return

    # ---- 模式3：同步指定股票 ----
    stocks = args.stock or (DEFAULT_STOCKS if args.all else [])
    if not stocks:
        parser.print_help()
        return

    # 规范化股票代码
    stocks = [normalize_stock_code(s) for s in stocks]

    # 解析日期
    start = _parse_date(args.start) if args.start else None
    end = _parse_date(args.end) if args.end else None

    if args.start and start is None or args.end and end is None:
        return  # 日期格式错误，已打印错误信息

    # 打印配置并执行同步
    _print_sync_config(stocks, args.freq, start, end)
    sync_all(stocks, args.freq, start, end)
    print("Done.")


if __name__ == "__main__":
    main()
