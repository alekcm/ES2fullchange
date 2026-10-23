"""python3 tools/fields.py  - dumps the fields of SimulationProperties/* (property-name constants) as Type::Field lines."""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from clrboot import *
m = read_module(MANAGED + "/Assembly-CSharp.dll")
for t in m.GetTypes():
    if t.FullName.startswith("SimulationProperties/"):
        for f in t.Fields: print(t.FullName + "::" + f.Name)
