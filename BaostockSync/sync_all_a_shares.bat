@echo off
chcp 65001 >nul

cd /d "%~dp0"

echo Starting 10 PowerShell windows for parallel K-line sync...
echo Each window will claim SyncStatus=0 stocks from StockInfo independently.

for /l %%i in (1,1,10) do (
    start "Sync Worker %%i" powershell -NoExit -ExecutionPolicy Bypass -File "%~dp0sync_worker.ps1"
)

echo All 10 PowerShell windows started.
pause
