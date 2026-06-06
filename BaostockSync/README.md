# BaostockSync

对接 Baostock 历史行情数据，同步 5分钟 / 30分钟 / 日线 K线到 SQL Server。

## 环境

- Python 3.11+
- SQL Server (已存在 `StockDb` 及 `Kline_5m` / `Kline_30m` / `Kline_1d` 表)

## 安装

```bash
pip install -r requirements.txt
```

## 用法

### 同步单只股票单周期

```bash
python main.py --stock sh.600000 --freq 1d --start 2024-01-01 --end 2024-06-01
```

### 同步多只股票多周期

```bash
python main.py --stock sh.600000 sh.600519 sz.000001 --freq 5m 30m 1d
```

### 同步默认列表全部周期（增量）

```bash
python main.py --all
```

## 数据库连接

修改 `config.py` 中的 `DB_CONFIG`：

```python
DB_CONFIG = {
    "driver": "ODBC Driver 17 for SQL Server",
    "server": r".\SQLEXPRESS",
    "database": "StockDb",
    "trusted_connection": "yes",
}
```

## 增量同步

- 首次运行会拉取 `2020-01-01` 至今的全部数据
- 后续运行自动从数据库最新时间继续增量同步
- 重复数据通过唯一索引自动跳过（`Symbol + TradeTime`）
