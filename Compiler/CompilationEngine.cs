using System;
using System.IO;
using System.Linq;

namespace Compiler {
    /* Effects the actual compilation output
     */

    class CompilationEngine {
        Tokenizer tokenizer;
        FileStream outFile;
        StreamWriter outWriter;
        SymbolTable table;
        VMWriter mWriter;

        string className;
        string[] op = { "+", "-", "*", "/", "&", "|", "<", ">", "=", "&lt;", "&gt;", "&amp;" };
        int nArgs;
        string returnType = string.Empty;
        int count = -1;
        int count2 = -1;
   

        public CompilationEngine(Tokenizer foo, string path) {
            tokenizer = foo;
            path = path.Substring(0, path.LastIndexOf('.')) + ".vm";

            table = new SymbolTable();
            mWriter = new VMWriter(path);
            CompileClass();
        }

        public void CompileClass() {
            tokenizer.Advance();
            tokenizer.Advance();

            className = tokenizer.Identifier();

            tokenizer.Advance();
            tokenizer.Advance();
            while (tokenizer.TokenType() == typeOfToken.KEYWORD && (tokenizer.KeyWord().Equals("static") || tokenizer.KeyWord().Equals("field")))
                CompileClassVarDec();
            while (tokenizer.TokenType() == typeOfToken.KEYWORD && (tokenizer.KeyWord().Equals("constructor") || tokenizer.KeyWord().Equals("function") || tokenizer.KeyWord().Equals("method")))
                CompileSubroutine();
        }

        public void CompileClassVarDec() {
            kind foo = GetKind(tokenizer.KeyWord());
            tokenizer.Advance();
            string type;
            if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                type = tokenizer.KeyWord();
            } else {
                type = tokenizer.Identifier();
            }
            tokenizer.Advance();

            table.Define(tokenizer.Identifier(), type, foo);

            tokenizer.Advance();
            while (tokenizer.Symbol().Equals(",")) {
                tokenizer.Advance();
                table.Define(tokenizer.Identifier(), type, foo);
                tokenizer.Advance();
            }
            tokenizer.Advance();
        }

        public void CompileSubroutine() {
            table.StartSubroutine();
            count = count2 = -1;

            string morc = tokenizer.KeyWord();

            tokenizer.Advance();

            returnType = tokenizer.TokenType() == typeOfToken.KEYWORD ? tokenizer.KeyWord() : tokenizer.Identifier();

            tokenizer.Advance();
            string subroutineName = tokenizer.Identifier();
            
            tokenizer.Advance();
            tokenizer.Advance();
            if (morc.Equals("method")) 
                table.Define("this", className, kind.ARG);
            CompileParameterList();
            tokenizer.Advance();
            tokenizer.Advance();
            int nLocals = 0;
            while (tokenizer.TokenType() == typeOfToken.KEYWORD && tokenizer.KeyWord().Equals("var")) {
                nLocals += CompileVarDec();
            }
            mWriter.WriteFunction(className + '.' + subroutineName, nLocals);

            if (morc.Equals("method")) {
                mWriter.WritePush("argument", 0);
                mWriter.WritePop("pointer", 0);
            }

            if (morc.Equals("constructor")) {
                mWriter.WritePush("constant", table.VarCount(kind.FIELD));
                mWriter.WriteCall("Memory.alloc", 1);
                mWriter.WritePop("pointer", 0);
            }

            CompileStatements(); 
            tokenizer.Advance();
        }

        public void CompileParameterList() {
            if (tokenizer.TokenType() == typeOfToken.KEYWORD || tokenizer.TokenType() == typeOfToken.IDENTIFIER) {
                string type;
                if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                    type = tokenizer.KeyWord();
                } else {
                    type = tokenizer.Identifier();
                }
                tokenizer.Advance();
                table.Define(tokenizer.Identifier(), type, kind.ARG);
                tokenizer.Advance();
                while (tokenizer.Symbol().Equals(",")) {
                    
                    tokenizer.Advance();
                    if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                        type = tokenizer.KeyWord();
                    } else {
                        type = tokenizer.Identifier();
                    }
                    tokenizer.Advance();
                    table.Define(tokenizer.Identifier(), type, kind.ARG);                  
                    tokenizer.Advance();
                }
            }
        }

        public int CompileVarDec() {
            int result = 1;
            tokenizer.Advance();
            string type;
            if (tokenizer.TokenType() == typeOfToken.KEYWORD) 
                type = tokenizer.KeyWord();
            else type = tokenizer.Identifier();
            tokenizer.Advance();
            table.Define(tokenizer.Identifier(), type, kind.VAR);
            tokenizer.Advance();        
            while (tokenizer.Symbol().Equals(",")) {
                result++;
                tokenizer.Advance();
                table.Define(tokenizer.Identifier(), type, kind.VAR);
                tokenizer.Advance();
            }
            tokenizer.Advance();
            return result;
        }

        public void CompileStatements() {
            while (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                switch (tokenizer.KeyWord()) {
                    case ("do"): CompileDo(); break;
                    case ("let"): CompileLet(); break;
                    case ("while"): CompileWhile(); break;
                    case ("return"): CompileReturn(); break;
                    case ("if"): CompileIf(); break;
                }
            }
        }

        public void CompileDo() {
            tokenizer.Advance();
            string bar = tokenizer.Identifier();
            tokenizer.Advance();
            CompileSubroutineCall(bar);
            mWriter.WritePop("temp", 0);
            tokenizer.Advance();
        }

        public void CompileExpressionList() {
            if (!(tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals(")"))) {
                CompileExpression();
                nArgs++;
                while (tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals(",")) {
                    nArgs++;
                    tokenizer.Advance();
                    CompileExpression();
                }
            }
        }

        public void CompileLet() {
            bool k = false;
            tokenizer.Advance();
            string nameOfVariable = tokenizer.Identifier();
            tokenizer.Advance();
            if (tokenizer.Symbol().Equals("[")) {
                tokenizer.Advance();
                CompileExpression();
                mWriter.WritePush(GetSegment(table.KindOf(nameOfVariable)), table.IndexOf(nameOfVariable));
                mWriter.WriteArithmetic("+");
                tokenizer.Advance();
                k = !k;
            }
            tokenizer.Advance();
            CompileExpression();
            
            if (k) {
                mWriter.WritePop("temp", 0);
                mWriter.WritePop("pointer", 1);
                mWriter.WritePush("temp", 0);
                mWriter.WritePop("that", 0);
            } else
                mWriter.WritePop(GetSegment(table.KindOf(nameOfVariable)), table.IndexOf(nameOfVariable));
            tokenizer.Advance();       
        }

        public void CompileWhile() {
            int k = NextCount("while");
            mWriter.WriteLabel("WHILE_EXP" + k);
            tokenizer.Advance();
            tokenizer.Advance();
            CompileExpression();
            mWriter.WriteArithmetic("~");
            mWriter.WriteIf("WHILE_END" + k);
            tokenizer.Advance();
            tokenizer.Advance();
            CompileStatements();
            mWriter.WriteGoto("WHILE_EXP" + k);
            mWriter.WriteLabel("WHILE_END" + k);
            tokenizer.Advance();
        }

        public void CompileReturn() {
            tokenizer.Advance();
            if (!(tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals(";")))
                CompileExpression();
            if (returnType.Equals("void")) {
                mWriter.WritePush("constant", 0);
            }
            mWriter.WriteReturn();
            tokenizer.Advance();
        }

        public void CompileIf() {
            int k = NextCount("if");
            tokenizer.Advance();
            tokenizer.Advance();
            CompileExpression();
            tokenizer.Advance();
            tokenizer.Advance();
            mWriter.WriteIf("IF_TRUE" + k);
            mWriter.WriteGoto("IF_FALSE" + k);
            mWriter.WriteLabel("IF_TRUE" + k);
            CompileStatements();
            tokenizer.Advance();
            if (tokenizer.TokenType() == typeOfToken.KEYWORD && tokenizer.KeyWord().Equals("else")) {
                mWriter.WriteGoto("IF_END" + k);
                mWriter.WriteLabel("IF_FALSE" + k);
                tokenizer.Advance();
                tokenizer.Advance();
                CompileStatements();
                tokenizer.Advance();
                mWriter.WriteLabel("IF_END" + k);
            } else {
                mWriter.WriteLabel("IF_FALSE" + k);
            }
        }

        public void CompileExpression() {
            CompileTerm();
            string command;
            while (tokenizer.TokenType() == typeOfToken.SYMBOL && op.Contains(tokenizer.Symbol())) {
                command = tokenizer.Symbol();
                tokenizer.Advance();
                CompileTerm();
                mWriter.WriteArithmetic(command);
            }      
        }
        
        public void CompileTerm() {
            if (tokenizer.TokenType() == typeOfToken.INT_CONST) {
                mWriter.WritePush("constant", tokenizer.IntVal());
                tokenizer.Advance();
            } else if (tokenizer.TokenType() == typeOfToken.STRING_CONST) {
                mWriter.WritePush("constant", tokenizer.stringVal().Length);
                mWriter.WriteCall("String.new", 1);
                for (int i = 0; i < tokenizer.stringVal().Length; i++) {
                    mWriter.WritePush("constant", tokenizer.stringVal()[i]);
                    mWriter.WriteCall("String.appendChar", 2);
                }
                tokenizer.Advance();
            } else if (tokenizer.TokenType() == typeOfToken.SYMBOL) {
                if (tokenizer.Symbol().Equals("(")) {
                    tokenizer.Advance();
                    CompileExpression(); 
                    tokenizer.Advance();
                } else if (tokenizer.Symbol().Equals("-")) {
                    tokenizer.Advance();
                    CompileTerm();
                    mWriter.WriteArithmetic("neg");
                } else if (tokenizer.Symbol().Equals("~")) {
                    tokenizer.Advance();
                    CompileTerm();
                    mWriter.WriteArithmetic("~");
                }
                else {
                    tokenizer.Advance();
                    CompileTerm();
                }
            } else if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                switch (tokenizer.KeyWord()) {
                    case "null": mWriter.WritePush("constant", 0); break;
                    case "false": mWriter.WritePush("constant", 0); break;
                    case "true": mWriter.WritePush("constant", 0); mWriter.WriteArithmetic("~"); break;
                    case "this": mWriter.WritePush("pointer", 0); break;
                }
                tokenizer.Advance();
            } else {
                string bar = tokenizer.Identifier();
                if (table.KindOf(bar) != kind.NONE && (char.IsLower(table.TypeOf(bar)[0]) || table.KindOf(bar) == kind.STATIC || table.TypeOf(bar).Equals("Array")))
                    mWriter.WritePush(GetSegment(table.KindOf(bar)), table.IndexOf(bar));

                tokenizer.Advance();
                if (tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals("[")) {
                    tokenizer.Advance();
                    CompileExpression();
                    mWriter.WriteArithmetic("+");
                    mWriter.WritePop("pointer", 1);
                    mWriter.WritePush("that", 0);
                    tokenizer.Advance();
                } else if (tokenizer.TokenType() == typeOfToken.SYMBOL && (tokenizer.Symbol().Equals("(") || tokenizer.Symbol().Equals("."))) {
                    CompileSubroutineCall(bar);
                }
            }
        }

        public void CompileSubroutineCall(string bar) {
            nArgs = 0;
            if (tokenizer.Symbol().Equals("(")) {
                nArgs = 1;
                tokenizer.Advance();
                mWriter.WritePush("pointer", 0);
                CompileExpressionList();
                mWriter.WriteCall(className + "." + bar, nArgs);
                tokenizer.Advance();
            } else {
                if (char.IsLower(bar[0])) {
                    nArgs++;
                    mWriter.WritePush(GetSegment(table.KindOf(bar)), table.IndexOf(bar));
                    bar = table.TypeOf(bar);              
                }
                bar += '.';

                tokenizer.Advance();
                string foo = tokenizer.Identifier();
                tokenizer.Advance();
                tokenizer.Advance();

                CompileExpressionList();

                mWriter.WriteCall(bar + foo, nArgs);

                tokenizer.Advance();
            }
        }

        private kind GetKind(string str) {
            switch (str) {
                case "var": return kind.VAR;
                case "static": return kind.STATIC;
                case "field": return kind.FIELD;
                default: return kind.ARG;
            }
        }
        
        private string GetSegment(kind k) {
            switch (k) {
                case kind.ARG: return "argument";
                case kind.STATIC: return "static";
                case kind.VAR: return "local";
                case kind.FIELD: return "this";
            }
            throw new Exception();
        }

        private int NextCount(string str) {
            if (str.Equals("while"))
                return ++count;
            else return ++count2;
        }


        public void Close() {
            mWriter.Close();
        }
    }
}
