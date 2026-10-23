# compile references (Assembly-CSharp referenced from a publicized copy in /tmp/ref)
REFS="-r:/tmp/ref/Assembly-CSharp.dll"
for d in Assembly-CSharp-firstpass Amplitude Amplitude.AI.Common UnityEngine UnityEngine.UI mscorlib System System.Core System.Xml; do REFS="$REFS -r:$MANAGED/$d.dll"; done
export REFS
