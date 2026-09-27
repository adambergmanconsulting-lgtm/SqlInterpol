param(
    [string]$NeedsJson = $env:NEEDS_JSON
)

if ([string]::IsNullOrWhiteSpace($NeedsJson)) {
    Write-Error "No Needs JSON context provided!"
    exit 1
}

$needsResults = $NeedsJson | ConvertFrom-Json

# Check if any job failed or was cancelled
$failedJobs = $needsResults.PSObject.Properties | Where-Object { 
    $_.Value.result -in @('failure', 'cancelled') 
}

if ($failedJobs) {
    $failedNames = ($failedJobs | ForEach-Object { $_.Name }) -join ', '
    Write-Error "One or more database E2E tests failed or were cancelled: $failedNames"
    exit 1
}

Write-Host "All requested E2E database tests completed successfully."