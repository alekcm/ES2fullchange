# source this: sets MONO + helper functions (see tools/setup.sh)
export MONOROOT=/tmp/mono/linux-mono-6.10.0.104-master
export PATH=$MONOROOT/bin:$PATH
export MONO_PATH=$MONOROOT/lib/mono/4.5
MANAGED=$(cd "$(dirname "${BASH_SOURCE[0]}")/../Managed" && pwd)
csc() { mono $MONOROOT/lib/mono/4.5/csc.exe -nologo -noconfig "$@"; }
