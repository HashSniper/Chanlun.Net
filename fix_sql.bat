@echo off
chcp 65001 >nul

:: 检查管理员权限
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] 请以管理员身份运行此脚本!
    pause
    exit /b 1
)

echo ============================================
echo   修复 IIS 500 错误 - SQL Server 权限
echo ============================================
echo.

set SQL_SERVER=.\SQLEXPRESS
set DATABASE=StockDb

:: 创建临时 SQL 脚本
echo -- 创建 IIS AppPool 登录 > %TEMP%\fix_iis.sql
echo IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'IIS APPPOOL\Chanlun.API') >> %TEMP%\fix_iis.sql
echo BEGIN >> %TEMP%\fix_iis.sql
echo     CREATE LOGIN [IIS APPPOOL\Chanlun.API] FROM WINDOWS; >> %TEMP%\fix_iis.sql
echo     PRINT '登录已创建'; >> %TEMP%\fix_iis.sql
echo END >> %TEMP%\fix_iis.sql
echo ELSE >> %TEMP%\fix_iis.sql
echo BEGIN >> %TEMP%\fix_iis.sql
echo     PRINT '登录已存在'; >> %TEMP%\fix_iis.sql
echo END >> %TEMP%\fix_iis.sql
echo. >> %TEMP%\fix_iis.sql

echo -- 创建数据库用户 >> %TEMP%\fix_iis.sql
echo USE [%DATABASE%]; >> %TEMP%\fix_iis.sql
echo IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'IIS APPPOOL\Chanlun.API') >> %TEMP%\fix_iis.sql
echo BEGIN >> %TEMP%\fix_iis.sql
echo     CREATE USER [IIS APPPOOL\Chanlun.API] FOR LOGIN [IIS APPPOOL\Chanlun.API]; >> %TEMP%\fix_iis.sql
echo     PRINT '用户已创建'; >> %TEMP%\fix_iis.sql
echo END >> %TEMP%\fix_iis.sql
echo ELSE >> %TEMP%\fix_iis.sql
echo BEGIN >> %TEMP%\fix_iis.sql
echo     PRINT '用户已存在'; >> %TEMP%\fix_iis.sql
echo END >> %TEMP%\fix_iis.sql
echo. >> %TEMP%\fix_iis.sql

echo -- 授予权限 >> %TEMP%\fix_iis.sql
echo ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\Chanlun.API]; >> %TEMP%\fix_iis.sql
echo PRINT '已授予 db_owner 权限'; >> %TEMP%\fix_iis.sql
echo. >> %TEMP%\fix_iis.sql

echo -- Network Service (备选) >> %TEMP%\fix_iis.sql
echo IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'NT AUTHORITY\NETWORK SERVICE') >> %TEMP%\fix_iis.sql
echo BEGIN >> %TEMP%\fix_iis.sql
echo     CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS; >> %TEMP%\fix_iis.sql
echo     PRINT 'Network Service 登录已创建'; >> %TEMP%\fix_iis.sql
echo END >> %TEMP%\fix_iis.sql
echo USE [%DATABASE%]; >> %TEMP%\fix_iis.sql
echo IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'NT AUTHORITY\NETWORK SERVICE') >> %TEMP%\fix_iis.sql
echo BEGIN >> %TEMP%\fix_iis.sql
echo     CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE]; >> %TEMP%\fix_iis.sql
echo     PRINT 'Network Service 用户已创建'; >> %TEMP%\fix_iis.sql
echo END >> %TEMP%\fix_iis.sql
echo ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\NETWORK SERVICE]; >> %TEMP%\fix_iis.sql
echo PRINT '已授予 Network Service db_owner 权限'; >> %TEMP%\fix_iis.sql

echo [1/2] 执行 SQL 脚本...
sqlcmd -S %SQL_SERVER% -i %TEMP%\fix_iis.sql
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] SQL 执行失败!
    echo 请确保 SQL Server 正在运行，且 sqlcmd 可用。
    pause
    exit /b 1
)

echo.
echo [2/2] 重启 IIS...
%windir%\system32\inetsrv\appcmd.exe stop site "Chanlun.API" >nul 2>&1
%windir%\system32\inetsrv\appcmd.exe start site "Chanlun.API" >nul 2>&1
if %errorlevel% equ 0 (
    echo IIS 站点已重启。
) else (
    echo IIS 站点重启可能失败，请手动检查。
)

del %TEMP%\fix_iis.sql >nul 2>&1

echo.
echo ============================================
echo   修复完成!
echo ============================================
echo.
echo 请刷新浏览器测试: http://localhost:5000
echo.
pause
