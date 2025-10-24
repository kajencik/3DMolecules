# Fix broken terminal by killing stuck git processes
Write-Host "Killing any stuck git processes..." -ForegroundColor Yellow
Get-Process -Name git -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

Write-Host "Terminal should now be clear." -ForegroundColor Green
Write-Host "You can now run git commands normally." -ForegroundColor Green
