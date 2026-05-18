@echo off
chcp 65001 >nul
setlocal

rem ============================================
rem Config: change to your TDX dlls folder
rem ============================================
set "TDX_DLL_DIR=C:\new_tdx64\T0002\dlls"
set "DLL_NAME=NativeAdapter"

rem ============================================
rem Build Debug with full debug symbols
rem ============================================
echo [1/3] Building Debug...
cd /d "%~dp0"
dotnet publish -c Debug -r win-x64 -p:PublishAot=true
if errorlevel 1 (
    echo [ERROR] Build failed
    pause
    exit /b 1
)

rem ============================================
rem Copy DLL + PDB to TDX folder
rem ============================================
echo [2/3] Copying DLL + PDB...
copy /Y "bin\Debug\net10.0-windows\win-x64\native\%DLL_NAME%.dll" "%TDX_DLL_DIR%\"
copy /Y "bin\Debug\net10.0-windows\win-x64\native\%DLL_NAME%.pdb" "%TDX_DLL_DIR%\"
if errorlevel 1 (
    echo [WARN] Copy failed, check path or if TDX is running
    pause
    exit /b 1
)

rem ============================================
rem Done
rem ============================================
echo [3/3] Done
echo.
echo Files copied to: %TDX_DLL_DIR%
echo  - %DLL_NAME%.dll
echo  - %DLL_NAME%.pdb
echo.
echo ============================================
echo Next steps:
echo 1. Start TDX, open indicator that uses this DLL
echo 2. In VS: Debug - Attach to Process
echo 3. Select TdxW.exe, choose [Native] code type
echo 4. Switch stock or period to trigger calculation
echo ============================================
pause
