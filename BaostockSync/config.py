"""项目配置"""

DB_CONFIG = {
    "driver": "ODBC Driver 17 for SQL Server",
    "server": r".\SQLEXPRESS",
    "database": "StockDb",
    "trusted_connection": "yes",
}

DB_CONN_STR = (
    f"DRIVER={{{DB_CONFIG['driver']}}};"
    f"SERVER={DB_CONFIG['server']};"
    f"DATABASE={DB_CONFIG['database']};"
    f"Trusted_Connection={DB_CONFIG['trusted_connection']};"
)

# 同步的K线周期映射
# key: 业务标识, value: (baostock_frequency, db_table_name)
FREQUENCY_MAP = {
    "5m":  ("5",  "Kline_5m"),
    "30m": ("30", "Kline_30m"),
    "1d":  ("d",  "Kline_1d"),
}

# 默认同步的股票列表（Baostock 格式）
DEFAULT_STOCKS = [
    "sh.600000",  # 浦发银行
    "sh.600519",  # 贵州茅台
    "sz.000001",  # 平安银行
    "sz.000002",  # 万科A
]

# 每次同步的单只股票日期跨度（避免请求过大）
SYNC_BATCH_DAYS = {
    "5m": 30,
    "30m": 90,
    "1d": 365,
}

# 单条 INSERT 的批量大小
BATCH_SIZE = 500
