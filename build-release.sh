#!/usr/bin/env bash
set -euo pipefail

rid="$1"
root="$(cd "$(dirname "$0")" && pwd)"
out="$root/bin/Release/net10.0"
stage="$(mktemp -d)"
trap 'rm -rf "$stage"' EXIT

dotnet publish "$root/CodeScan.csproj" -c Release -r "$rid" -o "$stage/$rid"
cp "$root/FilesToMoveToManaged.json" "$stage/"
mkdir -p "$out"
rm -f "$out/$rid.zip"
(cd "$stage" && zip -qr "$out/$rid.zip" .)
