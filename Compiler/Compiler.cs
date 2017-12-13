using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    class Compiler {
        static void Main(string[] args) {
            string[] allFiles = Directory.GetFiles(args[0]);
            string[] jackFiles = new string[allFiles.Length];

            int jackFilesLength = 0;
            for (int i = 0; i < allFiles.Length; i++)
                if (allFiles[i].EndsWith(".jack")) 
                    jackFiles[jackFilesLength++] = allFiles[i];;                

            for (int i = 0; i < jackFilesLength; i++) {
                Tokenizer tokenizer = new Tokenizer(jackFiles[i]);
                tokenizer.Advance();

                CompilationEngine compEng = new CompilationEngine(tokenizer, jackFiles[i]);
                tokenizer.Close();
                compEng.Close();
            } 
        }
    }
}
