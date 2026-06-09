#Requires -RunAsAdministrator

<#
.SYNOPSIS
    部署 Chanlun.API + Chanlun.Web 到 IIS
.DESCRIPTION
    1. 停止已有站点（如果存在）
    2. 创建/更新应用程序池（No Managed Code）
    3. 创建/更新 IIS 站点
    4. 启动站点
#>

$ErrorActionPreference = "Stop"

$siteName     = "Chanlun.API"
$appPoolName  = "Chanlun.API"
$publishPath  = "E:\Code\MyCode\ChanLun.Net\publish\Chanlun.API"
$port         = 5000
$hostName     = "*"

Import-Module WebAdministration -ErrorAction SilentlyContinue

function Test-Admin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal   = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Admin)) {
    Write-Host "[ERROR] 请以管理员身份运行 PowerShell!" -ForegroundColor Red
    Write-Host "右键 PowerShell -> 以管理员身份运行，然后执行本脚本。"
    pause
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Chanlun.API + Web IIS 部署" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 检查发布目录
if (-not (Test-Path $publishPath)) {
    Write-Host "[ERROR] 发布目录不存在: $publishPath" -ForegroundColor Red
    Write-Host "请先执行: dotnet publish Chanlun.API -c Release -o publish\Chanlun.API"
    pause
    exit 1
}
Write-Host "[1/6] 发布目录确认: $publishPath" -ForegroundColor Green

# 2. 检查 ASP.NET Core Hosting Bundle
$aspnetModule = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue
if (-not $aspnetModule) {
    Write-Host "[WARNING] AspNetCoreModuleV2 未找到!" -ForegroundColor Yellow
    Write-Host "         请下载并安装 ASP.NET Core Hosting Bundle:" -ForegroundColor Yellow
    Write-Host "         https://dotnet.microsoft.com/download/dotnet" -ForegroundColor Yellow
} else {
    Write-Host "[2/6] AspNetCoreModuleV2 已安装" -ForegroundColor Green
}

# 3. 停止站点（如果存在）
$site = Get-Website -Name $siteName -ErrorAction SilentlyContinue
if ($site) {
    Write-Host "[3/6] 停止现有站点: $siteName ..." -ForegroundColor Yellow
    Stop-Website -Name $siteName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
} else {
    Write-Host "[3/6] 站点不存在，准备新建 ..." -ForegroundColor Green
}

# 4. 创建/更新应用程序池
$pool = Get-Item -Path "IIS:\AppPools\$appPoolName" -ErrorAction SilentlyContinue
if (-not $pool) {
    Write-Host "[4/6] 创建应用程序池: $appPoolName ..." -ForegroundColor Green
    New-Item -Path "IIS:\AppPools\$appPoolName" -Force | Out-Null
} else {
    Write-Host "[4/6] 更新应用程序池: $appPoolName ..." -ForegroundColor Green
}

# 配置应用程序池
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "managedPipelineMode" -Value "Integrated"
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"

# 5. 创建/更新站点
if (-not $site) {
    Write-Host "[5/6] 创建 IIS 站点: $siteName ..." -ForegroundColor Green
    New-Website -Name $siteName `
        -PhysicalPath $publishPath `
        -ApplicationPool $appPoolName `
        -Port $port `
        -HostHeader $hostName -Force | Out-Null
} else {
    Write-Host "[5/6] 更新 IIS 站点: $siteName ..." -ForegroundColor Green
    Set-ItemProperty -Path "IIS:\Sites\$siteName" -Name "physicalPath" -Value $publishPath
    Set-ItemProperty -Path "IIS:\Sites\$siteName" -Name "applicationPool" -Value $appPoolName
}

# 6. 启动站点和应用程序池
Write-Host "[6/6] 启动应用程序池和站点 ..." -ForegroundColor Green
Start-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue
Start-Website -Name $siteName -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# 验证
$site = Get-Website -Name $siteName -ErrorAction SilentlyContinue
$pool = Get-WebAppPoolState -Name $appPoolName -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  部署完成!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  站点名称: $siteName" -ForegroundColor White
Write-Host "  物理路径: $publishPath" -ForegroundColor White
Write-Host "  访问地址: http://localhost:$port" -ForegroundColor White
Write-Host "  站点状态: $($site.State)" -ForegroundColor $(if($site.State -eq 'Started'){'Green'}else{'Red'})
Write-Host "  应用池状态: $($pool.Value)" -ForegroundColor $(if($pool.Value -eq 'Started'){'Green'}else{'Red'})
Write-Host ""

if ($site.State -ne 'Started') {
    Write-Host "[WARNING] 站点未启动，请检查 IIS 日志或事件查看器。" -ForegroundColor Yellow
}

pause
