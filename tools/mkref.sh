#!/bin/bash
# Builds /tmp/ref/Assembly-CSharp.dll: a publicized copy of the baseline assembly used only to compile src/GalacticCouncil.
set -e
cd "$(dirname "$0")/.."
source tools/env.sh; source tools/refs.sh
BASE_COMMIT=${BASE_COMMIT:-6ae43ff}
mkdir -p /tmp/build /tmp/pub /tmp/ref
for f in Managed/*.dll; do case "$f" in *"before remake"*) ;; *) cp "$f" /tmp/pub/ ;; esac; done
git show $BASE_COMMIT:Managed/Assembly-CSharp.dll > /tmp/pub/Assembly-CSharp.dll
cp Managed/Mono.Cecil.dll /tmp/build/
csc -codepage:65001 -nostdlib -out:/tmp/build/Publicize.exe -r:/tmp/build/Mono.Cecil.dll -r:$MANAGED/mscorlib.dll -r:$MANAGED/System.dll -r:$MANAGED/System.Core.dll tools/Patcher/Publicize.cs
mono /tmp/build/Publicize.exe /tmp/pub/Assembly-CSharp.dll /tmp/ref/Assembly-CSharp.dll
