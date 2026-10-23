#!/bin/bash
# Compile src/GalacticCouncil/*.cs into /tmp/build/GalacticCouncil.dll
set -e
cd "$(dirname "$0")/.."
source tools/env.sh; source tools/refs.sh
mkdir -p /tmp/build
csc -codepage:65001 -target:library -nostdlib -langversion:6 -debug- -optimize+ -out:/tmp/build/GalacticCouncil.dll $REFS $(find src/GalacticCouncil -name '*.cs') "$@"
