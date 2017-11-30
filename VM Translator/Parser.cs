using System;
using System.IO;

namespace VM_Translator {
    /*
     * Handles the parsing of a single .vm file, and encapsulates access to the input code.
     * It reads VM commands, parses them, and provides convenient access to their components.
     * In addition, it removes all white space and comments.
     */
    class Parser {
        FileStream fileOrig;
        FileStream fileCopy;
        StreamReader vmReader;
        StreamWriter vmWriter;
        string CurCommand { get; set; }

        public Parser(string fileName) {
            fileOrig = new FileStream(fileName, FileMode.Open);
            vmReader = new StreamReader(fileOrig);
            fileCopy = new FileStream("Copy.txt", FileMode.Create);
            vmWriter = new StreamWriter(fileCopy);

            while (HasMoreCommands()) {
                string startString = vmReader.ReadLine();
                string newString = string.Empty;

                short comment = 0;
                for (int i = 0; i < startString.Length && comment != 2; i++) {
                    if (startString[i] == '/') comment++;
                    else newString += startString[i];
                }
                if (!newString.Equals(string.Empty))
                    vmWriter.WriteLine(newString);
            }

            vmReader.Close();
            fileOrig.Close();
            vmWriter.Close();
            fileCopy.Close();

            fileCopy = new FileStream("Copy.txt", FileMode.Open);
            vmReader = new StreamReader(fileCopy);
        }

        public bool HasMoreCommands() {
            return !vmReader.EndOfStream;
        }

        public void Advance() {
            CurCommand = vmReader.ReadLine();
        }

        public typeOfCommand CommandType() {
            string[] command = CurCommand.Split(' ');
            switch (command[0]) {
                case "push": return typeOfCommand.C_PUSH; 
                case "pop": return typeOfCommand.C_POP;
                case "label": return typeOfCommand.C_LABEL;
                case "goto": return typeOfCommand.C_GOTO;
                case "if-goto": return typeOfCommand.C_IF;
                case "function": return typeOfCommand.C_FUNCTION;
                case "call": return typeOfCommand.C_CALL;
                case "return": return typeOfCommand.C_RETURN;
                default: return typeOfCommand.C_ARITHMETIC;
            }
        }

        public string Arg1() {
            string[] command = CurCommand.Split(' ');
            if (command.Length == 1) return command[0];
            return command[1];
        }

        public string Arg2() {
            string[] command = CurCommand.Split(' ');
            return command[2];
        }

        public void Close() {
            vmReader.Close();
            fileCopy.Close();
            File.Delete("Copy.txt");
        }
    }
}
