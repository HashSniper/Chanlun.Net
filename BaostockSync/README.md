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

### 同步全部 A 股股票和 ETF 的 K 线

```bash
python main.py --all-a-shares --freq 1d
```

> 该命令读取 `StockInfo` 表中的股票列表进行 K 线同步。请确保先运行 `--sync-stock-info` 初始化股票信息。
> `SyncStatus` 状态：0=未同步，1=同步中，2=已同步。只处理状态为 0 的股票。

### 多窗口并行同步 K 线

双击运行 `sync_all_a_shares.bat`，会同时打开 10 个 PowerShell 窗口并行同步 K 线数据。

每个窗口通过数据库原子认领（`READPAST` + `UPDATE TOP(1)`）独立获取 `SyncStatus=0` 的股票，
处理完成后将其置为 2，因此多个窗口不会重复处理同一只股票。

```bat
sync_all_a_shares.bat
```

实际执行的 PowerShell 脚本为 `sync_worker.ps1`，如需调整参数（如只同步日线），请修改该文件。

### 仅同步全部股票基本信息

```bash
python main.py --sync-stock-info
```

> 该命令只更新 `StockInfo` 表，不会同步 K 线数据。

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
