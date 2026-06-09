#Requires -RunAsAdministrator

<#
.SYNOPSIS
    修复 IIS 部署后 500 错误 —— SQL Server 权限问题
.DESCRIPTION
    为 IIS 应用程序池身份创建 SQL Server 登录并授权访问 StockDb
#>

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  修复 IIS 500 错误 (SQL 权限)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$appPoolName = "Chanlun.API"
$sqlServer   = ".\SQLEXPRESS"
$database    = "StockDb"

# 检查管理员权限
$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal   = New-Object Security.Principal.WindowsPrincipal($currentUser)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[ERROR] 请以管理员身份运行 PowerShell!" -ForegroundColor Red
    pause
    exit 1
}

# 方案一：为 IIS AppPool 创建 SQL Server 登录
Write-Host "[方案一] 为 IIS AppPool 创建 SQL Server 登录..." -ForegroundColor Yellow
Write-Host ""

$sqlScript = @"
-- 创建 IIS AppPool 登录
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'IIS APPPOOL\$appPoolName')
BEGIN
    CREATE LOGIN [IIS APPPOOL\$appPoolName] FROM WINDOWS;
    PRINT '登录 [IIS APPPOOL\$appPoolName] 已创建';
END
ELSE
BEGIN
    PRINT '登录 [IIS APPPOOL\$appPoolName] 已存在';
END

-- 在 StockDb 中创建用户
USE [$database];
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'IIS APPPOOL\$appPoolName')
BEGIN
    CREATE USER [IIS APPPOOL\$appPoolName] FOR LOGIN [IIS APPPOOL\$appPoolName];
    PRINT '用户 [IIS APPPOOL\$appPoolName] 已在 [$database] 中创建';
END
ELSE
BEGIN
    PRINT '用户 [IIS APPPOOL\$appPoolName] 已存在';
END

-- 授予 db_owner 权限
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\$appPoolName];
PRINT '已授予 db_owner 权限';

-- 同时授予 Network Service 权限（备选）
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'NT AUTHORITY\NETWORK SERVICE')
BEGIN
    CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS;
    PRINT '登录 [NT AUTHORITY\NETWORK SERVICE] 已创建';
END

USE [$database];
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'NT AUTHORITY\NETWORK SERVICE')
BEGIN
    CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];
    PRINT '用户 [NT AUTHORITY\NETWORK SERVICE] 已在 [$database] 中创建';
END
ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
PRINT '已授予 [NT AUTHORITY\NETWORK SERVICE] db_owner 权限';
"@

$sqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
$sqlScript | Out-File -FilePath $sqlFile -Encoding UTF8

Write-Host "执行 SQL 脚本..." -ForegroundColor Green
Write-Host ""

try {
    sqlcmd -S $sqlServer -i $sqlFile -b
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCCESS] SQL 权限配置完成!" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] SQL 脚本执行可能有问题，退出码: $LASTEXITCODE" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[ERROR] SQL 执行失败: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "请手动在 SQL Server Management Studio 中执行以下脚本：" -ForegroundColor Yellow
    Write-Host $sqlScript -ForegroundColor Cyan
}

Remove-Item $sqlFile -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "[方案二] 重启 IIS 站点..." -ForegroundColor Yellow
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (Get-Module WebAdministration) {
    Restart-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue
    Restart-Website -Name $appPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    $site = Get-Website -Name $appPoolName -ErrorAction SilentlyContinue
    if ($site) {
        Write-Host "站点状态: $($site.State)" -ForegroundColor $(if($site.State -eq 'Started'){'Green'}else{'Red'})
    }
} else {
    Write-Host "WebAdministration 模块未加载，请手动重启 IIS 站点。" -ForegroundColor Yellow
    Write-Host "在管理员 CMD 中执行: iisreset" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  修复完成!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "请刷新浏览器测试: http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "如果仍报错，请尝试以下备选方案：" -ForegroundColor Yellow
Write-Host "1. 在管理员 CMD 中执行: iisreset" -ForegroundColor Cyan
Write-Host "2. 或重启计算机" -ForegroundColor Cyan
Write-Host ""
pause
