$nxRoot = "C:\Program Files\Siemens\DesigncenterXNX2512"
$runJournal = Join-Path $nxRoot "NXBIN\run_journal.exe"
$scriptPath = Join-Path $PSScriptRoot "..\python\nx_create_point.py"

if (-not (Test-Path $runJournal)) {
    Write-Error "Cannot find run_journal.exe at $runJournal"
    exit 1
}

$resolvedScriptPath = (Resolve-Path $scriptPath).Path

Write-Host "Launching NX create point script..."
Write-Host "Journal runner: $runJournal"
Write-Host "Script: $resolvedScriptPath"

& $runJournal $resolvedScriptPath
$exitCode = $LASTEXITCODE

Write-Host "run_journal exit code: $exitCode"
exit $exitCode
