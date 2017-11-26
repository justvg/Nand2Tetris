using System;
using System.IO;

namespace Assembler {
    /* Encapsulates access to the input code 
     * Reads an assembly language command, parse it, and provides convenient access to the coomand's components
     * In addition, removes all white spaces and comments
     */
    class Parse {
        FileStream asmFileOrig;
        FileStream asmFileCopy;
        StreamReader asmStream;
        StreamWriter streamWriter;
        string CurCommand { get; set; }

        public Parse(string fileName) {
            asmFileOrig = new FileStream(fileName, FileMode.Open);
            asmFileCopy = new FileStream("Copy.txt", FileMode.Create);
            asmStream = new StreamReader(asmFileOrig);
            streamWriter = new StreamWriter(asmFileCopy);

            while (HasMoreCommands()) {
                string startString = asmStream.ReadLine();
                string newString = string.Empty;

                short comment = 0;
                for (int i = 0; i < startString.Length && comment != 2; i++) {
                    if (startString[i] != '/' && startString[i] != ' ')
                        newString += startString[i];
                    else if (startString[i] == '/')
                        comment++;
                }

                if (!newString.Equals(string.Empty))
                    streamWriter.WriteLine(newString);
            }

            asmStream.Close();
            asmFileOrig.Close();
            streamWriter.Close();
            asmFileCopy.Close();

            asmFileCopy = new FileStream("Copy.txt", FileMode.Open);
            asmStream = new StreamReader(asmFileCopy);
        }

        public Boolean HasMoreCommands() {
            return !asmStream.EndOfStream;
        }

        public void Advance() {
            CurCommand = asmStream.ReadLine();
        }

        public typeOfCommand CommandType() {
            switch (CurCommand[0]) {
                case '@': return typeOfCommand.A_COMMAND;
                case '(': return typeOfCommand.L_COMMAND;
                default: return typeOfCommand.C_COMMAND;
            }
        }

        public string Symbol() {
            if (CurCommand[0] == '@')
                return CurCommand.Substring(1);
            return CurCommand.Substring(1, CurCommand.Length - 2);
        }

        public string Dest() {
            if (CurCommand.IndexOf('=') > 0)
                return CurCommand.Substring(0, CurCommand.IndexOf('='));
            return String.Empty;
        }

        public string Comp() {
            int indexEqual = CurCommand.IndexOf('=');
            int indexSemicol = CurCommand.IndexOf(';');

            if (indexEqual > 0 && indexSemicol > 0)
                return CurCommand.Substring(indexEqual + 1, indexSemicol - indexEqual - 1);
            if (indexEqual > 0)
                return CurCommand.Substring(indexEqual + 1);
            if (indexSemicol > 0)
                return CurCommand.Substring(0, indexSemicol);
            return CurCommand;
        }

        public string Jump() {
            if (CurCommand.IndexOf(';') > 0)
                return CurCommand.Substring(CurCommand.IndexOf(';') + 1);
            return String.Empty;
        }

        public void toStartOfFile() {
            asmStream.BaseStream.Position = 0;
        }

        public void Close() {
            asmStream.Close();
            asmFileCopy.Close();
            File.Delete("Copy.txt");
        }
    }
}
    
    
