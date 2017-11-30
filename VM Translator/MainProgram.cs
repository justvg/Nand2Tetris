using System;
using System.IO;

namespace VM_Translator {
    enum typeOfCommand { C_ARITHMETIC, C_PUSH, C_POP, C_LABEL, C_GOTO, C_IF, C_FUNCTION, C_RETURN, C_CALL }

    class MainProgram {
        static void Main(string[] args) {
            string asmFileName = args[0].Substring(args[0].LastIndexOf('\\') + 1);
            CodeWriter codeWriter = new CodeWriter(args[0] + '\\' + asmFileName + ".asm");

            string[] allFiles = Directory.GetFiles(args[0]);
            string[] vmFiles = new string[allFiles.Length];
            int j = 0;
            for (int i = 0; i < allFiles.Length; i++)
                if (allFiles[i].Substring(allFiles[i].LastIndexOf('.')).Equals(".vm"))
                    vmFiles[j++] = allFiles[i];                                

            for (int i = 0; i < j; i++) {
                Parser parser = new Parser(vmFiles[i]);
                while (parser.HasMoreCommands()) {
                    parser.Advance();

                    typeOfCommand type = parser.CommandType();
                    string arg1, arg2;
                    arg1 = arg2 = string.Empty;

                    if (type != typeOfCommand.C_RETURN) {
                        arg1 = parser.Arg1();
                        if (type == typeOfCommand.C_PUSH || type == typeOfCommand.C_POP || type == typeOfCommand.C_FUNCTION || type == typeOfCommand.C_CALL)
                            arg2 = parser.Arg2();
                    }

                    if (type == typeOfCommand.C_ARITHMETIC)
                        codeWriter.WriteArithmetic(arg1);
                    else if (type == typeOfCommand.C_POP)
                        codeWriter.WritePushPop("pop", arg1, int.Parse(arg2));
                    else if (type == typeOfCommand.C_PUSH)
                        codeWriter.WritePushPop("push", arg1, int.Parse(arg2));
                }

                parser.Close();
            }
                     
            codeWriter.Close();
        }
    }
}
