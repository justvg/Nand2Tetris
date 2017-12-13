using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler {
    enum typeOfToken { KEYWORD, SYMBOL, IDENTIFIER, INT_CONST, STRING_CONST }

    class Tokenizer {
        FileStream fileOrig;
        FileStream fileWithoutComments;
        FileStream fileToken;
        StreamReader jackReader;
        StreamWriter jackWriter;
        string CurCommand;

        public Tokenizer(string fileName) {
            fileOrig = new FileStream(fileName, FileMode.Open);
            jackReader = new StreamReader(fileOrig);
            fileWithoutComments = new FileStream("Copy.txt", FileMode.Create);
            jackWriter = new StreamWriter(fileWithoutComments);

            // Delete comments from source program (don't change source program)
            bool inComment = false;
            while (HasMoreTokens()) {
                string startLine = jackReader.ReadLine();

                if (startLine.LastIndexOf("/*") > -1 && !inComment) {
                  inComment = true;
                }               
                if (startLine.LastIndexOf("//") > -1) {
                    startLine = startLine.Substring(0, startLine.IndexOf("//"));
                    
                }
                if (!string.IsNullOrWhiteSpace(startLine) && !inComment) {
                    startLine = startLine.Trim();
                    jackWriter.WriteLine(startLine);                 
                }
                if (startLine.LastIndexOf("*/") > -1 && inComment) {
                    inComment = false;
                }
            }

            jackReader.Close();
            fileOrig.Close();
            jackWriter.Close();
            fileWithoutComments.Close();

            fileWithoutComments = new FileStream("Copy.txt", FileMode.Open);
            jackReader = new StreamReader(fileWithoutComments);
            string fileOfTokens = fileName.Substring(0, fileName.LastIndexOf('.')) + "T.xml";
            fileToken = new FileStream(fileOfTokens, FileMode.Create);
            jackWriter = new StreamWriter(fileToken);

            jackWriter.WriteLine("<tokens>");

            // Tokenizing
            while (HasMoreTokens()) {
                char[] symbols = { ' ', '(', ')', ';', ',', '.', ']', '[', '-', '~' };
                string startLine = jackReader.ReadLine() + ' ';
                string strConst = string.Empty;
                short quCount = 0;
                string newToken = string.Empty;     

                for (int i = 0; i < startLine.Length; i++) {
                    if (startLine[i] == '"') {
                        newToken += startLine[i];
                        quCount++;
                        if (quCount == 2) {
                            quCount = 0;
                            WriteToXml(newToken);
                            newToken = string.Empty;
                        }
                    } else if (quCount == 1) {
                        newToken += startLine[i];
                    } else if (symbols.Contains(startLine[i])) {
                        if (!string.IsNullOrWhiteSpace(newToken))
                            WriteToXml(newToken);
                        if (!char.IsWhiteSpace(startLine[i]))
                            WriteToXml(startLine[i].ToString());
                        newToken = string.Empty;
                    } 
                    else newToken += startLine[i];
                }                  
            }

            jackWriter.WriteLine("</tokens>");

            jackReader.Close();
            fileOrig.Close();
            jackWriter.Close();
            fileToken.Close();
            File.Delete("Copy.txt");
            CurCommand = string.Empty;

            fileToken = new FileStream(fileOfTokens, FileMode.Open);
            jackReader = new StreamReader(fileToken);           
        }

        public bool HasMoreTokens() {
            return !jackReader.EndOfStream;
        }

        public void Advance() {
            if (HasMoreTokens()) {
                CurCommand = jackReader.ReadLine();
                if (CurCommand.Contains("<stringConstant>")) {
                    CurCommand = CurCommand.Substring(CurCommand.IndexOf('>') + 2, CurCommand.LastIndexOf('<') - CurCommand.IndexOf('>') - 3);
                    CurCommand = '"' + CurCommand.Trim() + '"';
                } else if (!(CurCommand.IndexOf('>') > CurCommand.LastIndexOf('<'))) {
                    CurCommand = CurCommand.Substring(CurCommand.IndexOf('>') + 2, CurCommand.LastIndexOf('<') - CurCommand.IndexOf('>') - 3);
                    CurCommand = CurCommand.Trim();
                }
            }
        }

        private void WriteToXml(string command) {
            CurCommand = command;
            switch (TokenType()) {
                case typeOfToken.KEYWORD: jackWriter.WriteLine("<keyword> " + command + " </keyword>"); break;
                case typeOfToken.SYMBOL:
                        if (command.Equals("<")) jackWriter.WriteLine("<symbol> &lt; </symbol>");
                        else if (command.Equals(">")) jackWriter.WriteLine("<symbol> &gt; </symbol>");
                        else if (command.Equals("&")) jackWriter.WriteLine("<symbol> &amp; </symbol>");
                        else jackWriter.WriteLine("<symbol> " + command + " </symbol>");
                        break;              
                case typeOfToken.INT_CONST: jackWriter.WriteLine("<integerConstant> " + command + " </integerConstant>"); break;
                case typeOfToken.STRING_CONST: command = command.Substring(1, command.Length - 1); jackWriter.WriteLine("<stringConstant> " + command + " </stringConstant>"); break;
                default: jackWriter.WriteLine("<identifier> " + command + " </identifier>"); break;
           } 
        }

        public typeOfToken TokenType() {
            string[] keywords = { "class", "constructor", "function", "method", "field", "static", "var", "int",
                                  "char", "boolean", "void", "true", "false", "null", "this", "let", "let", "do",
                                  "if", "else", "while", "return" };

            string[] symbols = { "{", "}", "(", ")", "[", "]", ".", ",", ";",
                                 "+", "-", "*", "/", "&", "|", "<", ">", "=", "~", "&lt;", "&gt;", "&quot;", "&amp;" };

            int n = 0;

            if (keywords.Contains(CurCommand)) return typeOfToken.KEYWORD;
            if (symbols.Contains(CurCommand)) return typeOfToken.SYMBOL;
            if (int.TryParse(CurCommand, out n) && n >= 0 && n <= 32767) return typeOfToken.INT_CONST;
            if (CurCommand[0].Equals('"')) return typeOfToken.STRING_CONST;
            return typeOfToken.IDENTIFIER;
        }

        public string KeyWord() {

            if (TokenType() == typeOfToken.KEYWORD) {
                return CurCommand;
            }
            throw new Exception();
        }

        public string Symbol() {
            
            if (TokenType() == typeOfToken.SYMBOL) {
                return CurCommand;
            }
            throw new Exception();
        }

        public string Identifier() {
           
            if (TokenType() == typeOfToken.IDENTIFIER) {
                return CurCommand;
            }
            throw new Exception();
        }

        public int IntVal() {
            
            if (TokenType() == typeOfToken.INT_CONST) {
                return int.Parse(CurCommand);
            }
            throw new Exception();
        }

        public string stringVal() {
            
            if (TokenType() == typeOfToken.STRING_CONST) {
                return CurCommand.Substring(1, CurCommand.Length - 3);
            }
            throw new Exception();
        }

        public void Close() {
            jackReader.Close();
            fileToken.Close();
        }
    }
}
