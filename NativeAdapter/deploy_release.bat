@echo off
chcp 65001 >nul
setlocal

rem ============================================
rem 配置：修改为你的通达信 DLL 目录
rem ============================================
set "TDX_DLL_DIR=C:\new_tdx64\T0002\dlls"
set "DLL_NAME=NativeAdapter"

rem ============================================
rem 编译 Release 版本（优化速度，体积更小）
rem ============================================
echo [1/2] 正在编译 Release 版本...
cd /d "%~dp0"
dotnet publish -c Release -r win-x64 -p:PublishAot=true
if errorlevel 1 (
    echo [错误] 编译失败
    pause
    exit /b 1
)

rem ============================================
rem 复制 DLL 到通达信目录
rem ============================================
echo [2/2] 正在复制 DLL 到通达信目录...
copy /Y "bin\Release\net10.0-windows\win-x64\native\%DLL_NAME%.dll" "%TDX_DLL_DIR%\"
if errorlevel 1 (
    echo [警告] 复制失败，请检查路径是否正确，或通达信是否正在运行
    pause
    exit /b 1
)

echo [完成] Release DLL 已部署到: %TDX_DLL_DIR%\%DLL_NAME%.dll
pause
