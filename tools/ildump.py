#!/usr/bin/env python3
"""Minimal .NET metadata + IL explorer built on dnfile/dncil.

Usage:
  ildump.py types <pattern> [...]        list types matching any pattern
  ildump.py type  <TypeName>             dump fields / methods / props of a type
  ildump.py il    <TypeName> <Method>    disassemble IL of method(s)
  ildump.py refs  <string>               find methods whose IL references a string/member name
  ildump.py str   <pattern>              find user strings
"""
import sys, os, re, functools

import dnfile
from dnfile.enums import MetadataTables
from dncil.cil.body import CilMethodBody
from dncil.cil.error import MethodBodyFormatError
from dncil.clr.token import Token, StringToken, InvalidToken
from dncil.cil.opcode import OpCodes

DEFAULT_ASM = os.environ.get("ASM", "Managed/Assembly-CSharp.dll")

ELEMENT_TYPES = {
    0x01: "void", 0x02: "bool", 0x03: "char", 0x04: "sbyte", 0x05: "byte",
    0x06: "short", 0x07: "ushort", 0x08: "int", 0x09: "uint", 0x0a: "long",
    0x0b: "ulong", 0x0c: "float", 0x0d: "double", 0x0e: "string",
    0x16: "TypedReference", 0x18: "IntPtr", 0x19: "UIntPtr", 0x1c: "object",
}


class Reader:
    def __init__(self, data):
        self.d = data
        self.i = 0

    def u8(self):
        v = self.d[self.i]
        self.i += 1
        return v

    def peek(self):
        return self.d[self.i] if self.i < len(self.d) else None

    def compressed(self):
        b = self.u8()
        if b & 0x80 == 0:
            return b
        if b & 0xC0 == 0x80:
            return ((b & 0x3F) << 8) | self.u8()
        return ((b & 0x1F) << 24) | (self.u8() << 16) | (self.u8() << 8) | self.u8()

    def token(self):
        v = self.compressed()
        tbl = [MetadataTables.TypeDef, MetadataTables.TypeRef, MetadataTables.TypeSpec][v & 3]
        return tbl, v >> 2

    @property
    def eof(self):
        return self.i >= len(self.d)


class Asm:
    def __init__(self, path):
        self.path = path
        self.pe = dnfile.dnPE(path)
        self.t = self.pe.net.mdtables
        self._typedef_names = {}
        self._nested = {}
        for r in (self.t.NestedClass.rows if self.t.NestedClass else []):
            self._nested[r.NestedClass.row_index] = r.EnclosingClass.row_index
        self.method_owner = {}      # methoddef row_index -> typedef row_index
        self.field_owner = {}
        self.method_name = {}
        self.field_name = {}
        for i, td in enumerate(self.t.TypeDef.rows, start=1):
            for m in td.MethodList:
                self.method_owner[m.row_index] = i
            for f in td.FieldList:
                self.field_owner[f.row_index] = i
        for i, m in enumerate(self.t.MethodDef.rows, start=1):
            self.method_name[i] = str(m.Name)
        for i, f in enumerate(self.t.Field.rows, start=1):
            self.field_name[i] = str(f.Name)

    # ---------- names ----------
    def typedef_name(self, idx):
        if idx in self._typedef_names:
            return self._typedef_names[idx]
        r = self.t.TypeDef.rows[idx - 1]
        name = str(r.TypeName)
        ns = str(r.TypeNamespace)
        if idx in self._nested:
            name = self.typedef_name(self._nested[idx]) + "/" + name
        elif ns:
            name = ns + "." + name
        self._typedef_names[idx] = name
        return name

    def typeref_name(self, idx):
        r = self.t.TypeRef.rows[idx - 1]
        ns, nm = str(r.TypeNamespace), str(r.TypeName)
        scope = r.ResolutionScope
        if scope and scope.table and scope.table.name == "TypeRef":
            return self.typeref_name(scope.row_index) + "/" + nm
        return (ns + "." + nm) if ns else nm

    def typespec_name(self, idx):
        r = self.t.TypeSpec.rows[idx - 1]
        return self.parse_type(Reader(r.Signature.value))

    def type_by_coded(self, tbl, idx):
        if tbl in (MetadataTables.TypeDef, "TypeDef"):
            return self.typedef_name(idx)
        if tbl in (MetadataTables.TypeRef, "TypeRef"):
            return self.typeref_name(idx)
        return self.typespec_name(idx)

    # ---------- signatures ----------
    def parse_type(self, r):
        et = r.u8()
        if et in ELEMENT_TYPES:
            return ELEMENT_TYPES[et]
        if et == 0x0f:
            return self.parse_type(r) + "*"
        if et == 0x10:
            return self.parse_type(r) + "&"
        if et == 0x11 or et == 0x12:  # valuetype / class
            tbl, idx = r.token()
            return self.type_by_coded(tbl, idx)
        if et == 0x13:
            return "!" + str(r.compressed())
        if et == 0x1e:
            return "!!" + str(r.compressed())
        if et == 0x1d:
            return self.parse_type(r) + "[]"
        if et == 0x14:  # array
            base = self.parse_type(r)
            rank = r.compressed()
            nsizes = r.compressed()
            for _ in range(nsizes):
                r.compressed()
            nlo = r.compressed()
            for _ in range(nlo):
                r.compressed()
            return base + "[" + ("," * (rank - 1)) + "]"
        if et == 0x15:  # generic inst
            base = self.parse_type(r)
            n = r.compressed()
            args = [self.parse_type(r) for _ in range(n)]
            return base + "<" + ", ".join(args) + ">"
        if et == 0x1b:  # fnptr
            self.parse_method_sig(r)
            return "fnptr"
        if et == 0x1f or et == 0x20:  # cmod_reqd / cmod_opt
            r.token()
            return self.parse_type(r)
        if et == 0x45:  # pinned-ish / sentinel handling
            return self.parse_type(r)
        return "et0x%02x" % et

    def parse_method_sig(self, r):
        flags = r.u8()
        gen = r.compressed() if flags & 0x10 else 0
        n = r.compressed()
        ret = self.parse_type(r)
        ps = []
        for _ in range(n):
            if r.peek() == 0x41:  # sentinel
                r.u8()
            ps.append(self.parse_type(r))
        return ret, ps, gen, (flags & 0x20) != 0

    def method_sig_str(self, mrow):
        try:
            ret, ps, gen, inst = self.parse_method_sig(Reader(mrow.Signature.value))
        except Exception as e:
            return "<sig error %s>" % e
        g = "<" + ",".join("T%d" % i for i in range(gen)) + ">" if gen else ""
        return "%s %s%s(%s)" % (ret, "", g, ", ".join(ps))

    def field_sig_str(self, frow):
        try:
            r = Reader(frow.Signature.value)
            r.u8()
            return self.parse_type(r)
        except Exception as e:
            return "<sig error %s>" % e

    # ---------- lookups ----------
    @functools.lru_cache(maxsize=None)
    def find_types(self, pattern):
        out = []
        for i in range(1, len(self.t.TypeDef.rows) + 1):
            n = self.typedef_name(i)
            if re.search(pattern, n, re.I):
                out.append((i, n))
        return out

    def type_index(self, name):
        exact = [i for i in range(1, len(self.t.TypeDef.rows) + 1)
                 if self.typedef_name(i) == name or self.typedef_name(i).endswith("." + name)
                 or self.typedef_name(i).split("/")[-1] == name]
        if exact:
            return exact
        return [i for i, n in self.find_types(re.escape(name))]

    def member_name(self, tbl, idx):
        if tbl == "MethodDef" or tbl == MetadataTables.MethodDef:
            owner = self.method_owner.get(idx)
            return "%s::%s" % (self.typedef_name(owner) if owner else "?", self.method_name.get(idx, "?"))
        if tbl == "Field" or tbl == MetadataTables.Field:
            owner = self.field_owner.get(idx)
            return "%s::%s" % (self.typedef_name(owner) if owner else "?", self.field_name.get(idx, "?"))
        if tbl == "MemberRef" or tbl == MetadataTables.MemberRef:
            r = self.t.MemberRef.rows[idx - 1]
            cls = r.Class
            if cls.table.name == "TypeRef":
                owner = self.typeref_name(cls.row_index)
            elif cls.table.name == "TypeDef":
                owner = self.typedef_name(cls.row_index)
            elif cls.table.name == "TypeSpec":
                owner = self.typespec_name(cls.row_index)
            else:
                owner = cls.table.name
            return "%s::%s" % (owner, str(r.Name))
        if tbl == "MethodSpec" or tbl == MetadataTables.MethodSpec:
            r = self.t.MethodSpec.rows[idx - 1]
            m = r.Method
            return self.member_name(m.table.name, m.row_index) + "<..>"
        if tbl == "TypeRef" or tbl == MetadataTables.TypeRef:
            return self.typeref_name(idx)
        if tbl == "TypeDef" or tbl == MetadataTables.TypeDef:
            return self.typedef_name(idx)
        if tbl == "TypeSpec" or tbl == MetadataTables.TypeSpec:
            return self.typespec_name(idx)
        return "%s[%d]" % (tbl, idx)

    def resolve_token(self, tok):
        if isinstance(tok, StringToken):
            try:
                return '"%s"' % self.pe.net.user_strings.get(tok.rid).value
            except Exception:
                return '"?"'
        table = MetadataTables(tok.table).name if tok.table in [e.value for e in MetadataTables] else str(tok.table)
        try:
            return self.member_name(table, tok.rid)
        except Exception as e:
            return "%s[%d]!%s" % (table, tok.rid, e)

    # ---------- IL ----------
    def method_body(self, mrow):
        if not mrow.Rva:
            return None
        try:
            return CilMethodBody(DnfileReader(self.pe, mrow.Rva))
        except MethodBodyFormatError:
            return None

    def disasm(self, mrow, show_offsets=True):
        body = self.method_body(mrow)
        if body is None:
            return "    <no body>"
        lines = []
        for insn in body.instructions:
            op = str(insn.opcode)
            operand = ""
            if insn.operand is not None:
                if isinstance(insn.operand, (Token, StringToken)):
                    operand = self.resolve_token(insn.operand)
                elif isinstance(insn.operand, list):
                    operand = "[" + ", ".join(hex(x) for x in insn.operand) + "]"
                else:
                    operand = str(insn.operand)
                    if insn.opcode in (OpCodes.Br, OpCodes.Br_S, OpCodes.Brtrue, OpCodes.Brtrue_S,
                                       OpCodes.Brfalse, OpCodes.Brfalse_S, OpCodes.Beq, OpCodes.Beq_S,
                                       OpCodes.Bge, OpCodes.Bge_S, OpCodes.Bgt, OpCodes.Bgt_S,
                                       OpCodes.Ble, OpCodes.Ble_S, OpCodes.Blt, OpCodes.Blt_S,
                                       OpCodes.Bne_Un, OpCodes.Bne_Un_S, OpCodes.Leave, OpCodes.Leave_S):
                        try:
                            operand = "IL_%04X" % (int(insn.operand) - body.offset)
                        except Exception:
                            pass
            pre = "    IL_%04X: " % (insn.offset - body.offset) if show_offsets else "    "
            lines.append("%s%-12s %s" % (pre, op, operand))
        return "\n".join(lines)


from dncil.cil.body.reader import CilMethodBodyReaderBase


class DnfileReader(CilMethodBodyReaderBase):
    """byte reader over the PE used by dncil"""

    def __init__(self, pe, rva):
        self.pe = pe
        self.offset = pe.get_offset_from_rva(rva)

    def read(self, n):
        data = self.pe.get_data(self.pe.get_rva_from_offset(self.offset), n)
        self.offset += n
        return data

    def tell(self):
        return self.offset

    def seek(self, p):
        self.offset = p
        return self.offset


def cmd_types(asm, args):
    for p in args:
        for i, n in asm.find_types(p):
            print(n)


def type_info(asm, idx):
    td = asm.t.TypeDef.rows[idx - 1]
    name = asm.typedef_name(idx)
    base = ""
    if td.Extends and td.Extends.table:
        base = asm.type_by_coded(td.Extends.table.name, td.Extends.row_index)
    print("=" * 100)
    print("type %s : %s" % (name, base))
    ifaces = []
    for r in (asm.t.InterfaceImpl.rows if asm.t.InterfaceImpl else []):
        if r.Class.row_index == idx:
            ifaces.append(asm.type_by_coded(r.Interface.table.name, r.Interface.row_index))
    if ifaces:
        print("  implements: " + ", ".join(ifaces))
    print("  -- fields --")
    for f in td.FieldList:
        fr = f.row
        print("    %-55s %s%s" % (str(fr.Name), asm.field_sig_str(fr),
                                  "  [static]" if fr.Flags.fdStatic else ""))
    print("  -- methods --")
    for m in td.MethodList:
        mr = m.row
        print("    [%d] %-50s %s%s" % (m.row_index, str(mr.Name), asm.method_sig_str(mr),
                                       "  [static]" if mr.Flags.mdStatic else ""))


def cmd_type(asm, args):
    for a in args:
        for idx in asm.type_index(a):
            type_info(asm, idx)


def cmd_il(asm, args):
    tname, mpat = args[0], args[1] if len(args) > 1 else ".*"
    for idx in asm.type_index(tname):
        td = asm.t.TypeDef.rows[idx - 1]
        for m in td.MethodList:
            mr = m.row
            if re.search(mpat, str(mr.Name), re.I):
                print("=" * 100)
                print("%s::%s %s" % (asm.typedef_name(idx), str(mr.Name), asm.method_sig_str(mr)))
                print(asm.disasm(mr))


def cmd_refs(asm, args):
    pat = re.compile(args[0], re.I)
    for idx in range(1, len(asm.t.TypeDef.rows) + 1):
        td = asm.t.TypeDef.rows[idx - 1]
        for m in td.MethodList:
            mr = m.row
            body = asm.method_body(mr)
            if not body:
                continue
            for insn in body.instructions:
                if insn.operand is not None and isinstance(insn.operand, (Token, StringToken)):
                    s = asm.resolve_token(insn.operand)
                    if pat.search(s):
                        print("%s::%s  ->  %s" % (asm.typedef_name(idx), str(mr.Name), s))
                        break


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return
    asm = Asm(os.environ.get("ASM", DEFAULT_ASM))
    cmd = sys.argv[1]
    args = sys.argv[2:]
    {"types": cmd_types, "type": cmd_type, "il": cmd_il, "refs": cmd_refs}[cmd](asm, args)


if __name__ == "__main__":
    main()
