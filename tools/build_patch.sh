#!/bin/bash
# Rebuilds Managed/Assembly-CSharp.dll = baseline (hacking fixes commit) + Galactic Council.
#   tools/build_patch.sh [outfile]
set -e
cd "$(dirname "$0")/.."
BASE_COMMIT=${BASE_COMMIT:-6ae43ff}
OUT=${1:-Managed/Assembly-CSharp.dll}
source tools/env.sh; source tools/refs.sh
mkdir -p /tmp/build
git show $BASE_COMMIT:Managed/Assembly-CSharp.dll > /tmp/build/base.dll
cp Managed/Mono.Cecil.dll /tmp/build/Mono.Cecil.dll
tools/build_council.sh 2>&1 | grep -v "warning" || true
csc -codepage:65001 -nostdlib -langversion:6 -out:/tmp/build/Patcher.exe -r:/tmp/build/Mono.Cecil.dll -r:$MANAGED/mscorlib.dll -r:$MANAGED/System.dll -r:$MANAGED/System.Core.dll tools/Patcher/Patcher.cs tools/Patcher/Cloner.cs
mono /tmp/build/Patcher.exe /tmp/build/base.dll /tmp/build/GalacticCouncil.dll "$OUT" "$MANAGED"
