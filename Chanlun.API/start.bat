@echo off
chcp 65001 >nul
REM 启动 Chanlun.API（ASP.NET Core）
dotnet run --project Chanlun.API.csproj
