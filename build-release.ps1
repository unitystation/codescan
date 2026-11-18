param([Parameter(Mandatory)] [string] $RuntimeIdentifier)
$ErrorActionPreference = "Stop"

$out = Join-Path $PSScriptRoot "bin/Release/net10.0"
$stage = Join-Path ([IO.Path]::GetTempPath()) ([IO.Path]::GetRandomFileName())

try {
	dotnet publish (Join-Path $PSScriptRoot "CodeScan.csproj") -c Release -r $RuntimeIdentifier -o (Join-Path $stage $RuntimeIdentifier)
	if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

	Copy-Item (Join-Path $PSScriptRoot "FilesToMoveToManaged.json") $stage
	New-Item -ItemType Directory -Force $out | Out-Null
	Compress-Archive -Path (Join-Path $stage "*") -DestinationPath (Join-Path $out "$RuntimeIdentifier.zip") -Force
}
finally {
	Remove-Item -Recurse -Force $stage -ErrorAction SilentlyContinue
}
