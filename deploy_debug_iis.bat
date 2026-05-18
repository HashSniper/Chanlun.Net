@echo off

:: Switch to script directory
pushd "%~dp0"

:: ============================================
:: Chanlun.API Debug IIS Deploy
:: ============================================

set "SITE_NAME=Chanlun.API"
set "PUBLISH_DIR=publish\Chanlun.API"
set "PROJECT_PATH=Chanlun.API\Chanlun.API.csproj"

:: Check admin
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Please run as Administrator!
    echo Right-click the bat file and select "Run as administrator".
    pause
    exit /b 1
)

:: Check dotnet
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] dotnet command not found. Please install .NET SDK.
    pause
    exit /b 1
)

echo ============================================
echo   Chanlun.API - Debug IIS Deploy
echo ============================================
echo.

:: Stop IIS site
echo [1/4] Stopping IIS site: %SITE_NAME% ...
%windir%\system32\inetsrv\appcmd.exe stop site "%SITE_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    echo       Site stopped.
) else (
    echo       Site not running or not found, continue...
)
timeout /t 1 /nobreak >nul

:: Clean old files
echo [2/4] Cleaning old publish files ...
if exist "%PUBLISH_DIR%" (
    rmdir /s /q "%PUBLISH_DIR%"
    echo       Old files cleaned.
) else (
    echo       Nothing to clean.
)

:: Publish Debug
echo [3/4] Publishing Debug version ...
dotnet publish "%PROJECT_PATH%" -c Debug -o "%PUBLISH_DIR%"
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)
echo       Publish success.

:: Start IIS site
echo [4/4] Starting IIS site: %SITE_NAME% ...
%windir%\system32\inetsrv\appcmd.exe start site "%SITE_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    echo       Site started.
) else (
    echo       Start failed, please check IIS Manager.
)

echo.
echo ============================================
echo   Deploy Complete!
echo ============================================
echo.
echo Next step: Debug in Visual Studio
echo --------------------------------------------
echo 1. Open ChanLun.Net.sln
echo 2. Set breakpoints in code
echo 3. Menu: Debug - Attach to Process (Ctrl+Alt+P)
echo 4. Find and select w3wp.exe, click Attach
echo 5. Call API in browser/Postman, breakpoint will hit
echo.
pause
