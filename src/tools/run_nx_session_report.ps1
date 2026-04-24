param(
    [Parameter(Mandatory = $true)]
    [string]$PartPath
)

$nxRoot = "C:\Program Files\Siemens\DesigncenterXNX2512"
$runJournal = Join-Path $nxRoot "NXBIN\run_journal.exe"
$scriptPath = Join-Path $PSScriptRoot "..\python\nx_session_report.py"

if (-not (Test-Path $runJournal)) {
    Write-Error "Cannot find run_journal.exe at $runJournal"
    exit 1
}

$resolvedPartPath = (Resolve-Path $PartPath).Path
$resolvedScriptPath = (Resolve-Path $scriptPath).Path

Write-Host "Launching NX session report script..."
Write-Host "Journal runner: $runJournal"
Write-Host "Script: $resolvedScriptPath"
Write-Host "Target part: $resolvedPartPath"

${env:NX_TARGET_PRT} = $resolvedPartPath
& $runJournal $resolvedScriptPath
$exitCode = $LASTEXITCODE
Remove-Item Env:NX_TARGET_PRT -ErrorAction SilentlyContinue

Write-Host "run_journal exit code: $exitCode"
exit $exitCode
