using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    enum kind { STATIC, FIELD, ARG, VAR, NONE }

    class SymbolTable {
        Dictionary<string, Tuple<string, kind, int>> classScope;
        Dictionary<string, Tuple<string, kind, int>> subroutineScope;
        int staticCount, fieldCount, argCount, varCount;

        public SymbolTable() {
            classScope = new Dictionary<string, Tuple<string, kind, int>>();
            subroutineScope = new Dictionary<string, Tuple<string, kind, int>>();

            staticCount = fieldCount = argCount = varCount = 0;
        }

        public void StartSubroutine() {
            subroutineScope.Clear();
            argCount = varCount = 0;
        }

        public void Define(string name, string type, kind foo) {
            switch (foo) {
                case kind.STATIC: classScope.Add(name, new Tuple<string, kind, int>(type, foo, staticCount++)); break;
                case kind.FIELD: classScope.Add(name, new Tuple<string, kind, int>(type, foo, fieldCount++)); break;
                case kind.ARG: subroutineScope.Add(name, new Tuple<string, kind, int>(type, foo, argCount++)); break;
                case kind.VAR: subroutineScope.Add(name, new Tuple<string, kind, int>(type, foo, varCount++)); break;
            }
        }

        public int VarCount(kind foo) {
            int result = 0;

            switch (foo) {
                case kind.STATIC: return staticCount;
                case kind.FIELD: return fieldCount;
                case kind.ARG: return argCount;
                case kind.VAR: return varCount;
            }

            return result;
        }

        public kind KindOf(string name) {
            if (subroutineScope.ContainsKey(name))
                return subroutineScope[name].Item2;
            if (classScope.ContainsKey(name))
                return classScope[name].Item2;
            return kind.NONE;
        }

        public string TypeOf(string name) {
            if (subroutineScope.ContainsKey(name))
                return subroutineScope[name].Item1;
            if (classScope.ContainsKey(name))
                return classScope[name].Item1;
            return string.Empty;
        }

        public int IndexOf(string name) {
            if (subroutineScope.ContainsKey(name))
                return subroutineScope[name].Item3;
            if (classScope.ContainsKey(name))
                return classScope[name].Item3;
            return -1;
        }
    }
}