# Incremental NuGet Restore Script for AspireApp
# This script restores projects one-by-one in dependency order with retry logic

param(
    [int]$MaxRetries = 3,
    [int]$RetryDelaySeconds = 5
)

# Set error action preference
$ErrorActionPreference = "Continue"

# ANSI color codes for better output
$ColorReset = "`e[0m"
$ColorGreen = "`e[32m"
$ColorYellow = "`e[33m"
$ColorRed = "`e[31m"
$ColorBlue = "`e[34m"
$ColorCyan = "`e[36m"

Write-Host "${ColorCyan}======================================${ColorReset}"
Write-Host "${ColorCyan}  Incremental NuGet Restore Script${ColorReset}"
Write-Host "${ColorCyan}======================================${ColorReset}`n"

# Define projects in dependency order (no dependencies first, heaviest last)
$projects = @(
    @{Name="AspireApp.Domain.Shared"; Path="AspireApp.Domain.Shared\AspireApp.Domain.Shared.csproj"; Priority=1},
    @{Name="AspireApp.ApiService.Domain"; Path="AspireApp.ApiService.Domain\AspireApp.ApiService.Domain.csproj"; Priority=2},
    @{Name="AspireApp.ApiService.Application"; Path="AspireApp.ApiService.Application\AspireApp.ApiService.Application.csproj"; Priority=3},
    @{Name="AspireApp.Twilio"; Path="AspireApp.Twilio\AspireApp.Twilio.csproj"; Priority=3},
    @{Name="AspireApp.Modules.ActivityLogs"; Path="AspireApp.Modules.ActivityLogs\AspireApp.Modules.ActivityLogs.csproj"; Priority=3},
    @{Name="AspireApp.Modules.FileUpload"; Path="AspireApp.Modules.FileUpload\AspireApp.Modules.FileUpload.csproj"; Priority=3},
    @{Name="AspireApp.ApiService.Infrastructure"; Path="AspireApp.ApiService.Infrastructure\AspireApp.ApiService.Infrastructure.csproj"; Priority=4},
    @{Name="AspireApp.ApiService.Notifications"; Path="AspireApp.ApiService.Notifications\AspireApp.ApiService.Notifications.csproj"; Priority=4},
    @{Name="AspireApp.ApiService.Presentation"; Path="AspireApp.ApiService.Presentation\AspireApp.ApiService.Presentation.csproj"; Priority=5},
    @{Name="AspireApp.ServiceDefaults"; Path="AspireApp.ServiceDefaults\AspireApp.ServiceDefaults.csproj"; Priority=5},
    @{Name="AspireApp.ApiService"; Path="AspireApp.ApiService\AspireApp.ApiService.csproj"; Priority=6},
    @{Name="AspireApp.AppHost"; Path="AspireApp.AppHost\AspireApp.AppHost.csproj"; Priority=7}
)

# Track results
$successCount = 0
$failedCount = 0
$skippedCount = 0
$failedProjects = @()

# Function to check if project needs restore
function Test-ProjectNeedsRestore {
    param([string]$ProjectPath)
    
    $projectDir = Split-Path -Parent $ProjectPath
    $assetsFile = Join-Path $projectDir "obj\project.assets.json"
    
    # If assets file doesn't exist, needs restore
    if (-not (Test-Path $assetsFile)) {
        return $true
    }
    
    # If assets file is older than csproj, needs restore
    $assetsTime = (Get-Item $assetsFile).LastWriteTime
    $projectTime = (Get-Item $ProjectPath).LastWriteTime
    
    return $projectTime -gt $assetsTime
}

# Function to restore a single project with retry logic
function Restore-ProjectWithRetry {
    param(
        [string]$ProjectName,
        [string]$ProjectPath,
        [int]$MaxAttempts
    )
    
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        Write-Host "`n${ColorBlue}[$attempt/$MaxAttempts]${ColorReset} Restoring ${ColorYellow}$ProjectName${ColorReset}..."
        
        $startTime = Get-Date
        
        # Execute restore with detailed verbosity on first attempt, minimal on retries
        $verbosity = if ($attempt -eq 1) { "normal" } else { "minimal" }
        
        $restoreOutput = & dotnet restore $ProjectPath --no-cache --force --verbosity $verbosity 2>&1
        $exitCode = $LASTEXITCODE
        
        $duration = (Get-Date) - $startTime
        $durationStr = "{0:N1}s" -f $duration.TotalSeconds
        
        if ($exitCode -eq 0) {
            Write-Host "${ColorGreen}[SUCCESS]${ColorReset} Successfully restored ${ColorYellow}$ProjectName${ColorReset} in $durationStr"
            return $true
        }
        else {
            Write-Host "${ColorRed}[FAILED]${ColorReset} Failed to restore ${ColorYellow}$ProjectName${ColorReset} (attempt $attempt)"
            
            # Check for specific error patterns
            $errorText = $restoreOutput | Out-String
            
            if ($errorText -match "timed out|timeout") {
                Write-Host "${ColorRed}  > Timeout detected${ColorReset}"
            }
            elseif ($errorText -match "forcibly closed|connection.*closed") {
                Write-Host "${ColorRed}  > Connection reset detected${ColorReset}"
            }
            elseif ($errorText -match "Unable to read data") {
                Write-Host "${ColorRed}  > Network read error detected${ColorReset}"
            }
            
            if ($attempt -lt $MaxAttempts) {
                $delay = $RetryDelaySeconds * $attempt  # Exponential backoff
                Write-Host "${ColorYellow}  > Waiting ${delay}s before retry...${ColorReset}"
                Start-Sleep -Seconds $delay
            }
            else {
                Write-Host "${ColorRed}  > All attempts exhausted${ColorReset}"
                Write-Host "`nError details:"
                $restoreOutput | Select-Object -Last 10 | ForEach-Object { Write-Host "  $_" }
            }
        }
    }
    
    return $false
}

# Main restore loop
Write-Host "Starting incremental restore of ${ColorCyan}$($projects.Count)${ColorReset} projects...`n"
Write-Host "Configuration:"
Write-Host "  Max retries per project: ${ColorCyan}$MaxRetries${ColorReset}"
Write-Host "  Initial retry delay: ${ColorCyan}${RetryDelaySeconds}s${ColorReset}"
Write-Host "  Retry strategy: ${ColorCyan}Exponential backoff${ColorReset}`n"

$totalStartTime = Get-Date

foreach ($project in $projects) {
    $projectName = $project.Name
    $projectPath = $project.Path
    
    # Check if project file exists
    if (-not (Test-Path $projectPath)) {
        Write-Host "${ColorRed}[ERROR]${ColorReset} Project file not found: ${ColorYellow}$projectPath${ColorReset}"
        $skippedCount++
        continue
    }
    
    # Check if restore is needed
    if (-not (Test-ProjectNeedsRestore $projectPath)) {
        Write-Host "${ColorGreen}[OK]${ColorReset} ${ColorYellow}$projectName${ColorReset} already restored (skipping)"
        $skippedCount++
        continue
    }
    
    # Attempt restore
    $success = Restore-ProjectWithRetry -ProjectName $projectName -ProjectPath $projectPath -MaxAttempts $MaxRetries
    
    if ($success) {
        $successCount++
    }
    else {
        $failedCount++
        $failedProjects += $projectName
    }
    
    # Small delay between projects to avoid overwhelming the connection
    if ($project -ne $projects[-1]) {
        Start-Sleep -Milliseconds 500
    }
}

# Summary
$totalDuration = (Get-Date) - $totalStartTime
$totalMinutes = [math]::Floor($totalDuration.TotalMinutes)
$totalSeconds = [math]::Floor($totalDuration.TotalSeconds % 60)

Write-Host "`n${ColorCyan}======================================${ColorReset}"
Write-Host "${ColorCyan}  Restore Summary${ColorReset}"
Write-Host "${ColorCyan}======================================${ColorReset}"
Write-Host "Total time: ${ColorCyan}${totalMinutes}m ${totalSeconds}s${ColorReset}"
Write-Host "${ColorGreen}Successful:${ColorReset} $successCount"
Write-Host "${ColorYellow}Skipped:${ColorReset} $skippedCount"
Write-Host "${ColorRed}Failed:${ColorReset} $failedCount"

if ($failedCount -gt 0) {
    Write-Host "`n${ColorRed}Failed projects:${ColorReset}"
    foreach ($failed in $failedProjects) {
        Write-Host "  - $failed"
    }
    Write-Host "`n${ColorYellow}Troubleshooting tips:${ColorReset}"
    Write-Host "  1. Check your VPN connection stability"
    Write-Host "  2. Try running the script again (it will skip successful projects)"
    Write-Host "  3. Try temporarily disconnecting from VPN"
    Write-Host "  4. Check available disk space (need ~2GB+ free)"
    Write-Host "  5. Verify internet connectivity: curl https://api.nuget.org/v3/index.json"
    exit 1
}
else {
    Write-Host "`n${ColorGreen}All projects restored successfully!${ColorReset}"
    Write-Host "You can now run: ${ColorCyan}dotnet build --no-restore${ColorReset}"
    exit 0
}

