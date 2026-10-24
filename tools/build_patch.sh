#!/bin/bash
# Rebuilds Managed/Assembly-CSharp.dll = baseline game assembly + Galactic Council.
#   tools/build_patch.sh [outfile]
set -e
cd "$(dirname "$0")/.."
OUT=${1:-Managed/Assembly-CSharp.dll}
BASE_COMMIT=${BASE_COMMIT:-0e0f215}   # "hacking fixes" baseline assembly (contains CustomHackingHelper, no Council)
BASELINE="Managed/Assembly-CSharp — before remake.dll"
source tools/env.sh; source tools/refs.sh
mkdir -p /tmp/build
# the baseline game assembly: an explicit BASE_COMMIT wins, else the pristine "before remake" copy
if [ -n "$BASE_COMMIT" ] && git show "$BASE_COMMIT:Managed/Assembly-CSharp.dll" > /tmp/build/base.dll 2>/dev/null; then
  echo "patch: baseline from git $BASE_COMMIT"
else
  cp "$BASELINE" /tmp/build/base.dll
  echo "patch: baseline from '$BASELINE'"
fi
cp Managed/Mono.Cecil.dll /tmp/build/Mono.Cecil.dll
tools/build_council.sh 2>&1 | grep -v "warning" || true
csc -codepage:65001 -nostdlib -langversion:6 -out:/tmp/build/Patcher.exe -r:/tmp/build/Mono.Cecil.dll -r:$MANAGED/mscorlib.dll -r:$MANAGED/System.dll -r:$MANAGED/System.Core.dll tools/Patcher/Patcher.cs tools/Patcher/Cloner.cs
mono /tmp/build/Patcher.exe /tmp/build/base.dll /tmp/build/GalacticCouncil.dll "$OUT" "$MANAGED"
