param(
    [Parameter(Mandatory = $true)]
    [string]$PartPath,

    [Parameter(Mandatory = $true)]
    [string]$ConfigPath,

    [switch]$SaveChanges
)

$nxRoot = "C:\Program Files\Siemens\DesigncenterXNX2512"
$runJournal = Join-Path $nxRoot "NXBIN\run_journal.exe"
$scriptPath = Join-Path $PSScriptRoot "..\python\nx_lens_mold_asphere_points.py"

if (-not (Test-Path $runJournal)) {
    Write-Error "Cannot find run_journal.exe at $runJournal"
    exit 1
}

$resolvedPartPath = (Resolve-Path $PartPath).Path
$resolvedConfigPath = (Resolve-Path $ConfigPath).Path
$resolvedScriptPath = (Resolve-Path $scriptPath).Path

Write-Host "Launching NX lens mold asphere points script..."
Write-Host "Journal runner: $runJournal"
Write-Host "Script: $resolvedScriptPath"
Write-Host "Target part: $resolvedPartPath"
Write-Host "Config: $resolvedConfigPath"
Write-Host "Save changes: $SaveChanges"

${env:NX_TARGET_PRT} = $resolvedPartPath
${env:NX_LENS_CONFIG} = $resolvedConfigPath
${env:NX_SAVE_CHANGES} = if ($SaveChanges) { "1" } else { "0" }
& $runJournal $resolvedScriptPath
$exitCode = $LASTEXITCODE
Remove-Item Env:NX_TARGET_PRT -ErrorAction SilentlyContinue
Remove-Item Env:NX_LENS_CONFIG -ErrorAction SilentlyContinue
Remove-Item Env:NX_SAVE_CHANGES -ErrorAction SilentlyContinue

Write-Host "run_journal exit code: $exitCode"
exit $exitCode
