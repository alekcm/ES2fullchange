"""Bootstrap: load .NET Core 3.1 (from PyPI `dotnetcore2`) + Mono.Cecil via pythonnet.

Setup (once):
    pip install --target /tmp/pylib pythonnet clr-loader
    pip download --no-deps -d /tmp/pipdl dotnetcore2 && unzip -oq /tmp/pipdl/dotnetcore2-*.whl -d /tmp/dnc
"""
import sys, os
sys.path.insert(0, "/tmp/pylib")
ROOT = "/tmp/dnc/dotnetcore2/bin"
REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
MANAGED = os.path.join(REPO, "Managed")
os.chmod(ROOT + "/dotnet", 0o755)
os.environ["DOTNET_ROOT"] = ROOT
os.environ["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "1"
from pythonnet import load
load("coreclr", dotnet_root=ROOT)
import clr
clr.AddReference(os.path.join(MANAGED, "Mono.Cecil.dll"))
import Mono.Cecil as C
import Mono.Cecil.Cil as CIL
import System

def read_module(path, read_symbols=False):
    res = C.DefaultAssemblyResolver()
    res.AddSearchDirectory(MANAGED)
    p = C.ReaderParameters()
    p.AssemblyResolver = res
    return C.ModuleDefinition.ReadModule(path, p)
