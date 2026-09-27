param(
    [string]$Target,
    [string]$ComposeFile = "docker-compose.e2e.yml",
    [string]$OutputFile = $env:GITHUB_OUTPUT
)

$targetClean = ($Target ?? '').Trim()

# Resolve dialects: if target is empty, whitespace, or "all", query docker-compose for defined services
if ([string]::IsNullOrWhiteSpace($targetClean) -or $targetClean -eq 'all') {
    $rawServices = & docker compose -f $ComposeFile config --services
    $dialects = $rawServices | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() }
} 
else {
    # Split comma-separated targets and trim whitespace
    $dialects = $targetClean.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

# Convert string array to compact JSON (e.g., ["PostgreSQL","SqlServer"])
$jsonArray = $dialects | ConvertTo-Json -Compress

# Write output variable to GitHub Actions step environment
if (-not [string]::IsNullOrWhiteSpace($OutputFile)) {
    "dialects=$jsonArray" | Out-File -FilePath $OutputFile -Encoding utf8 -Append
}

Write-Host "Resolved E2E Test Matrix: $jsonArray"