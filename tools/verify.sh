#!/bin/bash
# Mono-JITs every method of the freshly built Managed/Assembly-CSharp.dll (compile/IL sanity only, no game code runs).
set -e
cd "$(dirname "$0")/.."
source tools/env.sh
mkdir -p /tmp/vd
for f in Managed/*.dll; do case "$f" in *mscorlib*|*/System*|*Mono.Security*|*Mono.Posix*|*Mono.Data.Tds*|*"before remake"*|*Assembly-CSharp.dll) ;; *) cp "$f" /tmp/vd/ ;; esac; done
cp Managed/Assembly-CSharp.dll /tmp/vd/
csc -codepage:65001 -out:/tmp/vd/Verify.exe -r:System.Core.dll tools/Verify/Verify.cs 2>&1 | grep -E "error" || true
cd /tmp/vd && MONO_PATH=/tmp/vd mono Verify.exe /tmp/vd/Assembly-CSharp.dll 2>&1 | tail -${1:-5}
