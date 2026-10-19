#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"

OUT="../Analyzers"
dotnet build -c Release

mkdir -p "$OUT"
cp bin/Release/netstandard2.0/AceLand.EventDriven.SourceGenerator.dll "$OUT/"

echo "✔ copied to $OUT/AceLand.EventDriven.SourceGenerator.dll"
echo "  → switch to Unity, wait for recompile, then run"
echo "    Tools ▸ AceLand ▸ EventDriven ▸ Fix Analyzer Import Settings"