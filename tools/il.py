"""Tiny Cecil-based CIL dumper.  usage: python3 tools/il.py [-d dll] Type [Method|*] | -f substring | -t typesubstr"""
import sys
sys.path.insert(0, __file__.rsplit("/",1)[0])
from clrboot import *

def all_types(m):
    for t in m.Types:
        yield from _walk(t)
def _walk(t):
    yield t
    for n in t.NestedTypes:
        yield from _walk(n)

def dump_method(md, out=sys.stdout):
    out.write("  %s %s(%s)\n" % (md.ReturnType.FullName, md.Name, ", ".join(p.ParameterType.Name + " " + p.Name for p in md.Parameters)))
    if not md.HasBody: return
    b = md.Body
    out.write("    // maxstack=%d locals=[%s]\n" % (b.MaxStackSize, ", ".join(v.VariableType.Name for v in b.Variables)))
    for i in b.Instructions:
        out.write("    %s\n" % i)
    for h in b.ExceptionHandlers:
        out.write("    // EH %s try %s-%s handler %s-%s\n" % (h.HandlerType, h.TryStart, h.TryEnd, h.HandlerStart, h.HandlerEnd))

if __name__ == "__main__":
    a = sys.argv[1:]
    dll = "Assembly-CSharp.dll"
    if a and a[0] == "-d":
        dll = a[1]; a = a[2:]
    m = read_module(MANAGED + "/" + dll)
    if a[0] == "-t":
        for t in all_types(m):
            if a[1].lower() in t.FullName.lower(): print(t.FullName, len(t.Methods), "methods")
    elif a[0] == "-f":
        for t in all_types(m):
            for md in t.Methods:
                if a[1].lower() in md.Name.lower(): print(t.FullName + "::" + md.Name)
    elif a[0] == "-F":
        for t in all_types(m):
            for f in t.Fields:
                if a[1].lower() in f.Name.lower(): print(t.FullName + "::" + f.Name, f.FieldType.Name)
    else:
        for t in all_types(m):
            if t.FullName == a[0]:
                print(t.FullName, ":", t.BaseType.FullName if t.BaseType else "")
                if len(a) == 1:
                    for f in t.Fields: print("  F", f.FieldType.Name, f.Name)
                    for md in t.Methods: print("  M", md.ReturnType.Name, md.Name, "(", ", ".join(p.ParameterType.Name for p in md.Parameters), ")")
                else:
                    for md in t.Methods:
                        if a[1] == "*" or md.Name == a[1]: dump_method(md)
