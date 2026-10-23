#!/bin/sh
# Downloads a Linux Mono 6.10 distribution (includes Roslyn csc.exe, ilasm, monodis) into /tmp/mono.
set -e
mkdir -p /tmp/mono && cd /tmp/mono
[ -d linux-mono-6.10.0.104-master ] || { curl -sSL -o mono.tgz https://codeload.github.com/TCATools/linux-mono-6.10.0.104/tar.gz/refs/heads/master && tar xzf mono.tgz && chmod +x linux-mono-6.10.0.104-master/bin/*; }
