"""
项目配置

将所有配置项集中管理，便于维护和修改。
"""

from datetime import datetime

# ============ 数据库配置 ============
DB_CONFIG = {
    "driver": "ODBC Driver 17 for SQL Server",
    "server": ".",
    "database": "StockDb",
    "username": "sa",
    "password": "dd461233",
}

DB_CONN_STR = (
    f"DRIVER={{{DB_CONFIG['driver']}}};"
    f"SERVER={DB_CONFIG['server']};"
    f"DATABASE={DB_CONFIG['database']};"
    f"UID={DB_CONFIG['username']};"
    f"PWD={DB_CONFIG['password']};"
)

# ============ K线周期配置 ============
# key: 业务标识, value: (baostock_frequency, db_table_name)
FREQUENCY_MAP = {
    "5m": ("5", "Kline_5m"),
    "30m": ("30", "Kline_30m"),
    "1d": ("d", "Kline_1d"),
}

# ============ 同步配置 ============
# 默认同步的股票列表（Baostock 格式）
DEFAULT_STOCKS = [
    "sh.600000",  # 浦发银行
    "sh.600519",  # 贵州茅台
    "sz.000001",  # 平安银行
    "sz.000002",  # 万科A
]

# 单条 INSERT 的批量大小
BATCH_SIZE = 500

# 增量同步的起始日期
DEFAULT_SYNC_START_DATE = datetime(2021, 1, 1)

# ============ Baostock API 配置 ============
# API 并发控制（单线程运行时无实际影响，保留作为安全兜底）
BS_MAX_CONCURRENT_REQUESTS = 1

# 请求失败时的最大重试次数
BS_MAX_RETRIES = 5

# 重试间隔（秒）
BS_RETRY_DELAYS = (1, 2, 4, 8, 16)

# Baostock 错误码：成功
BS_ERROR_CODE_SUCCESS = "0"

# ============ T+0 ETF 识别配置 ============
# 代码前缀：这些段内的 ETF 通常支持 T+0
T0_ETF_PREFIXES = (
    "511",  # 货币 ETF、债券 ETF
    "513",  # 跨境 ETF
    "518",  # 黄金 ETF
)

# 名称关键字：出现以下关键字通常支持 T+0
T0_ETF_KEYWORDS = (
    "货币", "黄金", "债", "可转债", "短融", "跨境", "商品", "期货",
    "原油", "石油", "豆粕", "能源化工", "恒", "港", "H股", "中概",
    "美股", "纳指", "纳斯达克", "标普", "日经", "日本", "德国", "欧洲",
    "法国", "英国", "印度", "越南", "韩国", "新兴市场", "亚太", "全球",
    "国际", "海外", "境外", "QDII", "富时", "MSCI",
)

# 明确指定为 T+0 的 ETF 代码（纯数字，不带市场前缀）
T0_ETF_CODES = set()

# 明确指定为 T+1 的 ETF 代码（用于覆盖关键字误识别）
T1_ETF_CODES = set()

# ============ ETF 代码前缀 ============
ETF_CODE_PREFIXES = (
    "510", "511", "512", "513", "515", "516", "517", "518",
    "560", "561", "562", "563", "564", "565", "566", "567",
    "568", "569", "58",
    "159",
)

# ============ 股票代码前缀（用于判断市场） ============
SHANGHAI_CODE_PREFIXES = ("6", "68", "88")
