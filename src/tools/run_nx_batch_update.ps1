param(
    [Parameter(Mandatory = $true)]
    [string]$FolderPath,

    [ValidateSet("point", "datum-plane")]
    [string]$Operation = "point",

    [switch]$SaveChanges,

    [switch]$Recurse
)

$resolvedFolderPath = (Resolve-Path $FolderPath).Path

if (-not (Test-Path $resolvedFolderPath)) {
    Write-Error "Cannot find folder: $FolderPath"
    exit 1
}

$launcher = switch ($Operation) {
    "point" { Join-Path $PSScriptRoot "run_nx_create_point.ps1" }
    "datum-plane" { Join-Path $PSScriptRoot "run_nx_create_datum_plane.ps1" }
}

$searchArgs = @{
    Path = $resolvedFolderPath
    Filter = "*.prt"
    File = $true
}

if ($Recurse) {
    $searchArgs["Recurse"] = $true
}

$parts = Get-ChildItem @searchArgs | Sort-Object FullName

if (-not $parts) {
    Write-Host "No .prt files found in: $resolvedFolderPath"
    exit 0
}

$successCount = 0
$failedParts = @()

Write-Host "Batch operation: $Operation"
Write-Host "Folder: $resolvedFolderPath"
Write-Host "Files found: $($parts.Count)"
Write-Host "Save changes: $SaveChanges"
Write-Host ""

foreach ($part in $parts) {
    Write-Host "Processing: $($part.FullName)"

    $args = @(
        "-ExecutionPolicy", "Bypass",
        "-File", $launcher,
        "-PartPath", $part.FullName
    )

    if ($SaveChanges) {
        $args += "-SaveChanges"
    }

    & powershell @args
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        $successCount += 1
        Write-Host "Result: success"
    } else {
        $failedParts += $part.FullName
        Write-Host "Result: failed with exit code $exitCode"
    }

    Write-Host ""
}

Write-Host "Batch complete."
Write-Host "Succeeded: $successCount"
Write-Host "Failed: $($failedParts.Count)"

if ($failedParts.Count -gt 0) {
    Write-Host "Failed files:"
    $failedParts | ForEach-Object { Write-Host $_ }
    exit 1
}

exit 0
