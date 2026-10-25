#!/bin/bash
# Compile src/GalacticCouncil/*.cs into /tmp/build/GalacticCouncil.dll
set -e
cd "$(dirname "$0")/.."
source tools/env.sh; source tools/refs.sh
mkdir -p /tmp/build
# ENABLE_DIAGNOSTICS: Amplitude.Diagnostics.Log/LogWarning/LogError are [Conditional("ENABLE_DIAGNOSTICS")];
# without the symbol every Diagnostics call in the mod (ModPerf buckets, Perf stall lines, the build stamp,
# temple logs) is silently compiled out and the game's Diagnostics.html shows nothing of the mod.
csc -codepage:65001 -target:library -nostdlib -langversion:6 -debug- -optimize+ -define:ENABLE_DIAGNOSTICS -out:/tmp/build/GalacticCouncil.dll $REFS $(find src/GalacticCouncil -name '*.cs') "$@"
