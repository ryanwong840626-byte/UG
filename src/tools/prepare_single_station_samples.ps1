param(
    [Parameter(Mandatory = $true)]
    [string]$NcPath,

    [string]$OutputCsv = "",

    [string]$SummaryPath = "",

    [double]$StationXOffset = 197.0,

    [double]$XSplitThreshold = 120.0,

    [int]$SampleEvery = 80
)

$scriptPath = Join-Path $PSScriptRoot "..\python\nc_single_station.py"
$resolvedScriptPath = (Resolve-Path $scriptPath).Path
$resolvedNcPath = (Resolve-Path $NcPath).Path

if ([string]::IsNullOrWhiteSpace($OutputCsv)) {
    $OutputCsv = Join-Path $PSScriptRoot "..\..\assets\samples\single_station_samples.csv"
}

if ([string]::IsNullOrWhiteSpace($SummaryPath)) {
    $SummaryPath = Join-Path $PSScriptRoot "..\..\assets\samples\single_station_samples_summary.json"
}

$resolvedOutputCsv = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputCsv)
$resolvedSummaryPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($SummaryPath)

Write-Host "Preparing single-station NC samples..."
Write-Host "Script: $resolvedScriptPath"
Write-Host "NC: $resolvedNcPath"
Write-Host "Output CSV: $resolvedOutputCsv"
Write-Host "Summary: $resolvedSummaryPath"
Write-Host "Station X offset: $StationXOffset"
Write-Host "X split threshold: $XSplitThreshold"
Write-Host "Sample every: $SampleEvery"

python $resolvedScriptPath `
    --nc $resolvedNcPath `
    --csv $resolvedOutputCsv `
    --summary $resolvedSummaryPath `
    --station-x-offset $StationXOffset `
    --x-split-threshold $XSplitThreshold `
    --sample-every $SampleEvery

exit $LASTEXITCODE
