param(
    [Parameter(Mandatory = $true)][string]$GodotPath,
    [string]$DotnetPath = 'dotnet',
    [string]$ScratchRoot = ''
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (!$ScratchRoot) {
    $ScratchRoot = Join-Path (Split-Path -Parent $projectRoot) 'NeonRift_Work\work\GodotSmoke'
}
$testRoot = Join-Path $ScratchRoot ('NeonRiftSurvivalSmoke-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null

# Preserve the original working project and keep generated build/test files in a unique copy.
Get-ChildItem -LiteralPath $projectRoot -Force |
    Where-Object { $_.Name -notin @('.git', '.godot', 'bin', 'obj') } |
    Copy-Item -Destination $testRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SurvivalSmokeTest.cs') -Destination $testRoot
$smokeScene = @'
[gd_scene load_steps=2 format=3]
[ext_resource type="Script" path="res://SurvivalSmokeTest.cs" id="1"]
[node name="SurvivalSmokeTest" type="Node"]
script = ExtResource("1")
'@
[System.IO.File]::WriteAllText((Join-Path $testRoot 'SurvivalSmokeTest.tscn'), $smokeScene)

& $DotnetPath build (Join-Path $testRoot 'NeonRiftStage1.csproj') --verbosity minimal --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Godot smoke test build failed.' }
& $GodotPath --headless --editor --path $testRoot --import --quit
if ($LASTEXITCODE -ne 0) { throw 'Godot smoke test import failed.' }
$runtimeOutput = & $GodotPath --headless --path $testRoot --scene res://SurvivalSmokeTest.tscn --quit-after 600 2>&1
$runtimeExitCode = $LASTEXITCODE
$runtimeOutput | ForEach-Object { Write-Host $_ }
if ($runtimeExitCode -ne 0 -or !($runtimeOutput | Where-Object { "$_" -match '^PASS: \d+ Godot integration checks$' })) {
    throw 'Godot smoke test execution failed or did not complete all checks.'
}
Write-Host "Test copy retained at: $testRoot"
