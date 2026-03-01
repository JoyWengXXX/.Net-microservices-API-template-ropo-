#!/usr/bin/env pwsh
# ConfigurationHelper 測試腳本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ConfigurationHelper Container 測試" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$testImage = "egmiot-config-test:latest"
$workDir = "g:/Arcade/IOT/Web/egmiot_web/EgmIoT_WebApi"

# 1. 建置測試映像
Write-Host "1. 建置測試映像..." -ForegroundColor Green
docker build -t $testImage -f tests/ConfigurationHelper.Test/Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 建置失敗" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ 建置成功`n" -ForegroundColor Green

# 2. 測試預設環境 (dev)
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "測試 1: 預設環境 (dev)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
docker run --rm $testImage

Write-Host "`n"

# 3. 測試 interdev 環境
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "測試 2: interdev 環境" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
docker run --rm -e ASPNETCORE_ENVIRONMENT=interdev $testImage

Write-Host "`n"

# 4. 測試環境變數覆寫
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "測試 3: 環境變數覆寫" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
docker run --rm `
    -e ASPNETCORE_ENVIRONMENT=interdev `
    -e EGMIOT__RabbitMQServer__Password=CONTAINER_ENV_PASSWORD `
    $testImage

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "測試完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
