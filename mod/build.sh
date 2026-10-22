#!/usr/bin/env bash
# Локальная сборка плагина (то же самое, что делает GitHub Actions).
#
# Нужно: .NET SDK (любой 6/7/8) ИЛИ mono-devel, и папка Managed/ с DLL игры (она уже в репозитории).
# BepInEx-сборку положите рядом или укажите путь: BEPINEX_DIR=/путь/к/BepInEx/core ./mod/build.sh
#
#   ./mod/build.sh              -> dist/ES2.HackingModes.dll
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
mkdir -p dist build

BEPINEX_DIR="${BEPINEX_DIR:-}"
if [ -z "$BEPINEX_DIR" ]; then
  BEPINEX_DIR="$(find . -name 'BepInEx.dll' -printf '%h\n' 2>/dev/null | head -1 || true)"
fi
if [ -z "$BEPINEX_DIR" ]; then
  echo "Не найден BepInEx.dll. Скачайте BepInEx 5.4.x (x64) и задайте BEPINEX_DIR=путь/к/BepInEx/core" >&2
  exit 1
fi

REFS=(
  /r:Managed/mscorlib.dll
  /r:Managed/System.dll
  /r:Managed/System.Core.dll
  /r:Managed/UnityEngine.dll
  /r:Managed/Assembly-CSharp.dll
  /r:Managed/Assembly-CSharp-firstpass.dll
  /r:Managed/Amplitude.dll
  "/r:$BEPINEX_DIR/BepInEx.dll"
  "/r:$BEPINEX_DIR/0Harmony.dll"
)

if command -v dotnet >/dev/null 2>&1; then
  CSC="$(ls /usr/share/dotnet/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | head -1 || true)"
  [ -z "$CSC" ] && CSC="$(find "$(dirname "$(command -v dotnet)")" -name csc.dll -path '*Roslyn*' 2>/dev/null | head -1 || true)"
  if [ -n "$CSC" ]; then
    exec dotnet exec "$CSC" /noconfig /nostdlib+ /optimize+ /target:library /langversion:6 \
      /out:dist/ES2.HackingModes.dll "${REFS[@]}" mod/src/*.cs
  fi
fi

if command -v mcs >/dev/null 2>&1; then
  exec mcs -target:library -nostdlib -sdk:2 -optimize+ \
    -out:dist/ES2.HackingModes.dll "${REFS[@]}" mod/src/*.cs
fi

echo "Не найден ни .NET SDK (csc.dll), ни mono (mcs)." >&2
exit 1
