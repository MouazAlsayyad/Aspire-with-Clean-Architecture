# Stop running AspireApp processes before build
Write-Host "Checking for running AspireApp processes..." -ForegroundColor Yellow

$processes = @(
    "AspireApp.ApiService",
    "AspireApp.AppHost"
)

$stopped = $false

foreach ($processName in $processes) {
    $runningProcesses = Get-Process -Name $processName -ErrorAction SilentlyContinue
    
    if ($runningProcesses) {
        foreach ($proc in $runningProcesses) {
            Write-Host "  Stopping $processName (PID: $($proc.Id))..." -ForegroundColor Cyan
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            $stopped = $true
        }
    }
}

if ($stopped) {
    Write-Host "  Waiting for processes to terminate..." -ForegroundColor Cyan
    Start-Sleep -Seconds 1
    Write-Host "Processes stopped successfully." -ForegroundColor Green
} else {
    Write-Host "No running processes found." -ForegroundColor Gray
}

exit 0

