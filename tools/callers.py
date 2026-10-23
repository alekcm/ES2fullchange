"""python3 tools/callers.py <substring>  - lists methods whose IL has an operand containing the substring."""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from clrboot import *
m = read_module(MANAGED + "/Assembly-CSharp.dll")
pat = sys.argv[1]
for t in m.GetTypes():
    for md in t.Methods:
        if not md.HasBody: continue
        for i in md.Body.Instructions:
            if i.Operand is not None and pat in str(i.Operand):
                print(t.FullName, md.Name, str(i.Operand)[:140]); break
