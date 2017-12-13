using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    class CompilationEngine {
        Tokenizer tokenizer;
        FileStream outFile;
        StreamWriter outWriter;
        SymbolTable table;

        string className;
        string[] op = { "+", "-", "*", "/", "&", "|", "<", ">", "=", "&lt;", "&gt;", "&amp;" };

        public CompilationEngine(Tokenizer foo, string path) {
            tokenizer = foo;
            path = path.Substring(0, path.LastIndexOf('.')) + ".xml";
            outFile = new FileStream(path, FileMode.Create);
            outWriter = new StreamWriter(outFile);
            table = new SymbolTable();
            CompileClass();
        }

        public void CompileClass() {
            tokenizer.Advance();
            outWriter.WriteLine("<class>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            className = tokenizer.Identifier();
            outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " class </identifier>");
            tokenizer.Advance();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            while (tokenizer.TokenType() == typeOfToken.KEYWORD && (tokenizer.KeyWord().Equals("static") || tokenizer.KeyWord().Equals("field")))
                CompileClassVarDec();
            while (tokenizer.TokenType() == typeOfToken.KEYWORD && (tokenizer.KeyWord().Equals("constructor") || tokenizer.KeyWord().Equals("function") || tokenizer.KeyWord().Equals("method")))
                CompileSubroutine();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>"); 
            outWriter.WriteLine("</class>");
        }

        public void CompileClassVarDec() {
            outWriter.WriteLine("<classVarDec>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            kind foo = GetKind(tokenizer.KeyWord());
            tokenizer.Advance();
            string type;
            if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                type = tokenizer.KeyWord();
                outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            } else {
                type = tokenizer.Identifier();
                outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " class </identifier>");
            }
            tokenizer.Advance();

            table.Define(tokenizer.Identifier(), type, foo);
            outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " defined " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");

            tokenizer.Advance();
            while (tokenizer.Symbol().Equals(",")) {
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                table.Define(tokenizer.Identifier(), type, foo);
                outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " defined " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");
                tokenizer.Advance();
            }
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("</classVarDec>");
        }

        public void CompileSubroutine() {
            table.StartSubroutine();
            if (tokenizer.KeyWord().Equals("method")) 
                table.Define("this", className, kind.ARG);
            outWriter.WriteLine("<subroutineDec>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            if (tokenizer.TokenType() == typeOfToken.KEYWORD)
                outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            else outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " </identifier>");
            tokenizer.Advance();
            outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " subroutine </identifier>");
            tokenizer.Advance();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            CompileParameterList();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("<subroutineBody>");
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            while (tokenizer.TokenType() == typeOfToken.KEYWORD && tokenizer.KeyWord().Equals("var")) {
                CompileVarDec();
            }
            CompileStatements(); 
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("</subroutineBody>");
            outWriter.WriteLine("</subroutineDec>");
        }

        public void CompileParameterList() {
            outWriter.WriteLine("<parameterList>");
            if (tokenizer.TokenType() == typeOfToken.KEYWORD || tokenizer.TokenType() == typeOfToken.IDENTIFIER) {
                string type;
                if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                    type = tokenizer.KeyWord();
                    outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
                } else {
                    type = tokenizer.Identifier();
                    outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " </identifier>");
                }
                tokenizer.Advance();
                table.Define(tokenizer.Identifier(), type, kind.ARG);
                outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " defined " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");
                tokenizer.Advance();
                while (tokenizer.Symbol().Equals(",")) {
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                    if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                        type = tokenizer.KeyWord();
                        outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
                    } else {
                        type = tokenizer.Identifier();
                        outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " </identifier>");
                    }
                    tokenizer.Advance();
                    table.Define(tokenizer.Identifier(), type, kind.ARG);
                    outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " defined " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");
                    tokenizer.Advance();
                }
            }
            outWriter.WriteLine("</parameterList>");
        }

        public void CompileVarDec() {
            outWriter.WriteLine("<varDec>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            string type;
            if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                type = tokenizer.KeyWord();
                outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            } else {
                type = tokenizer.Identifier();
                outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " class </identifier>");
            }
            tokenizer.Advance();
            table.Define(tokenizer.Identifier(), type, kind.VAR);
            outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " defined " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");
            tokenizer.Advance();        
            while (tokenizer.Symbol().Equals(",")) {
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                table.Define(tokenizer.Identifier(), type, kind.VAR);
                outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " defined " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");
                tokenizer.Advance();
            }
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("</varDec>");
        }

        public void CompileStatements() {
            outWriter.WriteLine("<statements>");
            while (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                switch (tokenizer.KeyWord()) {
                    case ("do"): CompileDo(); break;
                    case ("let"): CompileLet(); break;
                    case ("while"): CompileWhile(); break;
                    case ("return"): CompileReturn(); break;
                    case ("if"): CompileIf(); break;
                }
            }
            outWriter.WriteLine("</statements>");
        }

        public void CompileDo() {
            outWriter.WriteLine("<doStatement>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            //subroutinecall
            string bar = tokenizer.Identifier();
            tokenizer.Advance();
            CompileSubroutineCall(bar);
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("</doStatement>");
        }

        public void CompileExpressionList() {
            outWriter.WriteLine("<expressionList>");
            if (!(tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals(")"))) {
                CompileExpression();
                while (tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals(",")) {
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                    CompileExpression();
                }
            }
            outWriter.WriteLine("</expressionList>");
        }

        public void CompileLet() {
            outWriter.WriteLine("<letStatement>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " " + table.KindOf(tokenizer.Identifier()) + " used " + table.IndexOf(tokenizer.Identifier()) + " </identifier>");
            tokenizer.Advance();
            if (tokenizer.Symbol().Equals("[")) {
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                CompileExpression();
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
            }
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            CompileExpression();          
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();       
            outWriter.WriteLine("</letStatement>");
        }

        public void CompileWhile() {
            outWriter.WriteLine("<whileStatement>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            CompileExpression();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            CompileStatements();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("</whileStatement>");
        }

        public void CompileReturn() {
            outWriter.WriteLine("<returnStatement>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            if (!(tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals(";")))
                CompileExpression();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("</returnStatement>");
        }

        public void CompileIf() {
            outWriter.WriteLine("<ifStatement>");
            outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
            tokenizer.Advance();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            CompileExpression();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            CompileStatements();
            outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
            tokenizer.Advance();
            if (tokenizer.TokenType() == typeOfToken.KEYWORD && tokenizer.KeyWord().Equals("else")) {
                outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
                tokenizer.Advance();
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                CompileStatements();
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
            }
            outWriter.WriteLine("</ifStatement>");
        }

        public void CompileExpression() {
            outWriter.WriteLine("<expression>");
            CompileTerm();
            while (tokenizer.TokenType() == typeOfToken.SYMBOL && op.Contains(tokenizer.Symbol())) {
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                CompileTerm();
            }
            outWriter.WriteLine("</expression>");
        }
        
        public void CompileTerm() {
            outWriter.WriteLine("<term>");
            if (tokenizer.TokenType() == typeOfToken.INT_CONST) {
                outWriter.WriteLine("<integerConstant> " + tokenizer.IntVal() + " </integerConstant>");
                tokenizer.Advance();
            } else if (tokenizer.TokenType() == typeOfToken.STRING_CONST) {
                outWriter.WriteLine("<stringConstant> " + tokenizer.stringVal() + " </stringConstant>");
                tokenizer.Advance();
            } else if (tokenizer.TokenType() == typeOfToken.SYMBOL) {
                if (tokenizer.Symbol().Equals("(")) {
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                    CompileExpression(); 
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                } else {
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                    CompileTerm();
                }
            } else if (tokenizer.TokenType() == typeOfToken.KEYWORD) {
                outWriter.WriteLine("<keyword> " + tokenizer.KeyWord() + " </keyword>");
                tokenizer.Advance();
            } else {
                string bar = tokenizer.Identifier();
                tokenizer.Advance();
                if (tokenizer.TokenType() == typeOfToken.SYMBOL && tokenizer.Symbol().Equals("[")) {
                    outWriter.WriteLine("<identifier> " + bar + " " + table.KindOf(bar) + " used " + table.IndexOf(bar) + " </identifier>");
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                    CompileExpression();
                    outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                    tokenizer.Advance();
                } else if (tokenizer.TokenType() == typeOfToken.SYMBOL && (tokenizer.Symbol().Equals("(") || tokenizer.Symbol().Equals("."))) {
                    CompileSubroutineCall(bar);
                }
            }
            outWriter.WriteLine("</term>");
        }

        public void CompileSubroutineCall(string bar) {
            if (tokenizer.Symbol().Equals("(")) {
                outWriter.WriteLine("<identifier> " + bar + " subroutine </identifier>");
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                CompileExpressionList();
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
            } else {
                if (char.IsUpper(bar[0]))
                    outWriter.WriteLine("<identifier> " + bar + " class </identifier>");
                else
                    outWriter.WriteLine("<identifier> " + bar + " " + table.KindOf(bar) + " used " + table.IndexOf(bar) + " </identifier>");
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                outWriter.WriteLine("<identifier> " + tokenizer.Identifier() + " subroutine </identifier>");
                tokenizer.Advance();
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
                CompileExpressionList();
                outWriter.WriteLine("<symbol> " + tokenizer.Symbol() + " </symbol>");
                tokenizer.Advance();
            }
        }

        public kind GetKind(string str) {
            switch (str) {
                case "var": return kind.VAR;
                case "static": return kind.STATIC;
                case "field": return kind.FIELD;
                default: return kind.ARG;
            }
        }

        public void Close() {
            outWriter.Close();
            outFile.Close();
        }
    }
}
