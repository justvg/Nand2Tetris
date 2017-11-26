using System.Collections.Generic;

namespace Assembler {
    class SymbolTable {
        /* Keeps a correspondence between symbolic labels and numeric addresses
         */
        Dictionary<string, int> table;

        public SymbolTable() { table = new Dictionary<string, int>(); }
        public SymbolTable(Dictionary<string, int> init) { table = new Dictionary<string, int>(init); }

        public void addEntry(string symbol, int address) {
            table.Add(symbol, address);
        }

        public bool Contains(string symbol) {
            return table.ContainsKey(symbol);
        }

        public int GetAddress(string symbol) {
            return table[symbol];
        }
    }
}
