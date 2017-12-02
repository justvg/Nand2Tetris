using System;
using System.IO;

namespace VM_Translator {
    /* Translates VM commands into Hack assembly code.
     */
    class CodeWriter {
        const string ADD = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "M=D+M";      
        const string SUB = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "M=M-D";
        const string NEG = "@SP\r\n" + "A=M-1\r\n" + "M=-M";
        const string AND = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "M=D&M";
        const string OR = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "M=D|M";
        const string NOT = "@SP\r\n" + "A=M-1\r\n" + "M=!M";
        const string PUSH = "@SP\r\n" + "A=M\r\n" + "M=D\r\n" + "@SP\r\n" + "M=M+1";
        const string POP = "@R13\r\n" + "M=D\r\n" + "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "@R13\r\n" + "A=M\r\n" + "M=D";

        FileStream asmFile;
        StreamWriter asmWriter;
        string name;
        int count = 0;
        string curFunction = string.Empty;

        public CodeWriter(string fileName) {
            asmFile = new FileStream(fileName, FileMode.Create);
            asmWriter = new StreamWriter(asmFile);
            name = fileName.Substring(fileName.LastIndexOf('\\') + 1, fileName.IndexOf('.') - fileName.LastIndexOf('\\') - 1);
        }

        public void WriteArithmetic(string command) {
            switch (command) {              
                case "add": asmWriter.WriteLine(ADD); break;
                case "sub": asmWriter.WriteLine(SUB); break;
                case "neg": asmWriter.WriteLine(NEG); break;
                case "and": asmWriter.WriteLine(AND); break;
                case "or": asmWriter.WriteLine(OR); break;
                case "not": asmWriter.WriteLine(NOT); break;
                case "eq": asmWriter.WriteLine(Eq()); break;
                case "gt": asmWriter.WriteLine(Gt()); break;
                case "lt": asmWriter.WriteLine(Lt()); break;
            }
        }

        public void WritePushPop(string command, string segment, int index) {
            if (command.Equals("push")) asmWriter.WriteLine(Push(segment, index));
            else asmWriter.WriteLine(Pop(segment, index));
        }

        public void WriteLabel(string label) {
            asmWriter.WriteLine('(' + curFunction + '$' + label + ')');
        }

        public void WriteGoto(string label) {
            asmWriter.WriteLine('@' + curFunction + '$' + label);
            asmWriter.WriteLine("0;JMP");
        }

        public void WriteIf(string label) {
            asmWriter.WriteLine("@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + '@' + curFunction + '$' + label + "\r\n" + "D;JNE");   
        }

        public void WriteFunction(string functionName, int nubLocals) {
            curFunction = functionName.Substring(0, functionName.LastIndexOf('.'));
            asmWriter.WriteLine('(' + functionName + ')');
            if (nubLocals > 0) {
                asmWriter.WriteLine("@SP\r\n" + "A=M");
                for (int i = 0; i < nubLocals; i++) {
                    asmWriter.WriteLine("M=0\r\n" + "A=A+1");
                }
                asmWriter.WriteLine("D=A\r\n" + "@SP\r\n" + "M=D");
            }
        }

        public void WriteCall(string functionName, int numArgs) {
            string k = NextCount();
            asmWriter.WriteLine("@RET." + k + "\r\n" + "D=A\r\n" + PUSH);
            asmWriter.WriteLine("@LCL\r\n" + "D=M\r\n" + PUSH);
            asmWriter.WriteLine("@ARG\r\n" + "D=M\r\n" + PUSH);
            asmWriter.WriteLine("@THIS\r\n" + "D=M\r\n" + PUSH);
            asmWriter.WriteLine("@THAT\r\n" + "D=M\r\n" + PUSH);
            asmWriter.WriteLine("D=M\r\n" + '@' + numArgs + "\r\n" + "D=D-A\r\n" + "@5\r\n" + "D=D-A\r\n" + "@ARG\r\n" + "M=D");
            asmWriter.WriteLine("@SP\r\n" + "D=M\r\n" + "@LCL\r\n" + "M=D");
            asmWriter.WriteLine("@" + functionName + "\r\n" + "0;JMP");
            asmWriter.WriteLine("(RET." + k + ')');
        }

        public void WriteReturn() {           
            asmWriter.WriteLine("@LCL\r\n" + "D=M\r\n" + "@frame\r\n" + "M=D");
            asmWriter.WriteLine("@5\r\n" + "A=D-A\r\n" + "D=M\r\n" + "@ret\r\n" + "M=D");
            asmWriter.WriteLine("@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "@ARG\r\n" + "A=M\r\n" + "M=D");
            asmWriter.WriteLine("@ARG\r\n" + "D=M+1\r\n" + "@SP\r\n" + "M=D");
            asmWriter.WriteLine("@frame\r\n" + "D=M\r\n" + "@1\r\n" + "D=D-A\r\n" + "A=D\r\n" + "D=M\r\n" + "@THAT\r\n" + "M=D");
            asmWriter.WriteLine("@frame\r\n" + "D=M\r\n" + "@2\r\n" + "D=D-A\r\n" + "A=D\r\n" + "D=M\r\n" + "@THIS\r\n" + "M=D");
            asmWriter.WriteLine("@frame\r\n" + "D=M\r\n" + "@3\r\n" + "D=D-A\r\n" + "A=D\r\n" + "D=M\r\n" + "@ARG\r\n" + "M=D");
            asmWriter.WriteLine("@frame\r\n" + "D=M\r\n" + "@4\r\n" + "D=D-A\r\n" + "A=D\r\n" + "D=M\r\n" + "@LCL\r\n" + "M=D");
            asmWriter.WriteLine("@ret\r\n" + "A=M\r\n" + "0;JMP");
        }

        public void WriteInit() {
            asmWriter.WriteLine("@256\r\n" + "D=A\r\n" + "@SP\r\n" + "M=D");
            WriteCall("Sys.init", 0);
        }

        public void Close() {
            asmWriter.Close();
            asmFile.Close();
        }

        string NextCount() {
            count++;
            return count.ToString();
        }

        string Push(string segment, int index) {
            switch (segment) {
                case "constant": return "@" + index + "\r\n" + "D=A\r\n" + PUSH;
                case "local": return "@LCL\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "A=D+A\r\n" + "D=M\r\n" + PUSH;
                case "argument": return "@ARG\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "A=D+A\r\n" + "D=M\r\n" + PUSH;
                case "this": return "@THIS\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "A=D+A\r\n" + "D=M\r\n" + PUSH;
                case "that": return "@THAT\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "A=D+A\r\n" + "D=M\r\n" + PUSH;
                case "pointer": if (index == 0) return "@THIS\r\n" + "D=M\r\n" + PUSH; else return "@THAT\r\n" + "D=M\r\n" + PUSH;
                case "temp": return "@R5\r\n" + "D=A\r\n" + "@" + index + "\r\n" + "A=D+A\r\n" + "D=M\r\n" + PUSH;
                case "static": return "@" + curFunction + "." + index + "\r\n" + "D=M\r\n" + PUSH;
                default: throw new Exception("BAD");
            }
        }

        string Pop(string segment, int index) {
            int forTemp = 5 + index;
            switch (segment) {
                case "local": return "@LCL\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "D=D+A\r\n" + POP;
                case "argument": return "@ARG\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "D=D+A\r\n" + POP;
                case "this": return "@THIS\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "D=D+A\r\n" + POP;
                case "that": return "@THAT\r\n" + "D=M\r\n" + "@" + index + "\r\n" + "D=D+A\r\n" + POP;
                case "pointer": if (index == 0) return "@THIS\r\n" + "D=A\r\n" + POP; else return "@THAT\r\n" + "D=A\r\n" + POP;
                case "temp": return "@R5\r\n" + "D=A\r\n" + "@" + index + "\r\n" + "D=D+A\r\n" + POP;
                case "static": return "@" + name + "." + index + "\r\n" + "D=A\r\n" + POP;
                default: throw new Exception("BAD");
            }
        }

        string Eq() {
            string n = NextCount();
            string k = NextCount();
            string s = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "D=D-M\r\n" + '@' + name + '.' + n + "\r\n" +
                       "D;JEQ\r\n" + "@SP\r\n" + "A=M-1\r\n" + "M=0\r\n" + '@' + name + '.' + k + "\r\n" + "0;JMP\r\n" +
                       '(' + name + '.' + n + ")\r\n" + "@SP\r\n" + "A=M-1\r\n" + "M=-1\r\n" + '(' + name + '.' + k + ")";
            return s;
        }

        string Gt() {
            string n = NextCount();
            string k = NextCount();
            string s = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "D=M-D\r\n" + '@' + name + '.' + n + "\r\n" +
                       "D;JGT\r\n" + "@SP\r\n" + "A=M-1\r\n" + "M=0\r\n" + '@' + name + '.' + k + "\r\n" + "0;JMP\r\n" +
                       '(' + name + '.' + n + ")\r\n" + "@SP\r\n" + "A=M-1\r\n" + "M=-1\r\n" + '(' + name + '.' + k + ")";
            return s;
        }

        string Lt() {
            string n = NextCount();
            string k = NextCount();
            string s = "@SP\r\n" + "AM=M-1\r\n" + "D=M\r\n" + "A=A-1\r\n" + "D=M-D\r\n" + '@' + name + '.' + n + "\r\n" +
                       "D;JLT\r\n" + "@SP\r\n" + "A=M-1\r\n" + "M=0\r\n" + '@' + name + '.' + k + "\r\n" + "0;JMP\r\n" +
                       '(' + name + '.' + n + ")\r\n" + "@SP\r\n" + "A=M-1\r\n" + "M=-1\r\n" + '(' + name + '.' + k + ")";
            return s;
        }
    }
}
