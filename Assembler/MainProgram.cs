using System;
using System.Collections.Generic;
using System.IO;

namespace Assembler {
    enum typeOfCommand { A_COMMAND, C_COMMAND, L_COMMAND }

    class MainProgram {
        private static readonly Dictionary<string, int> predefSymbols = new Dictionary<string, int> {
            {"SP", 0}, {"LCL", 1}, {"ARG", 2}, {"THIS", 3}, {"THAT", 4}, {"R0", 0}, {"R1", 1}, {"R2", 2},
            {"R3", 3}, {"R4", 4}, {"R5", 5}, {"R6", 6}, {"R7", 7}, {"R8", 8}, {"R9", 9}, {"R10", 10}, {"R11", 11},
            {"R12", 12}, {"R13", 13}, {"R14", 14}, {"R15", 15}, {"SCREEN", 16384}, {"KBD", 24576}
        };

        static string translateFromNumeric(int number) {
            string result = String.Empty;
            for (int i = 0; i < 15; i++)
                result += (number >> i) & 1;

            char[] binary = result.ToCharArray();
            Array.Reverse(binary);
            result = new string(binary);
            result = "0" + result;

            return result;
        }

        static void Main(string[] args) {
            Parse parse = new Parse(args[0] + ".asm");
            FileStream hackFile = new FileStream(args[0] + ".hack", FileMode.Create);
            StreamWriter hackWriter = new StreamWriter(hackFile);

            // Initialize
            SymbolTable table = new SymbolTable(predefSymbols);

            // First Pass
            int romAddress = 0;
            while(parse.HasMoreCommands()) {
                parse.Advance();
                if (parse.CommandType() == typeOfCommand.A_COMMAND || parse.CommandType() == typeOfCommand.C_COMMAND)
                    romAddress++;
                if (parse.CommandType() == typeOfCommand.L_COMMAND) {
                    table.addEntry(parse.Symbol(), romAddress);
                }
            }

            // Second Pass
            int ramAddress = 16;
            parse.toStartOfFile();
            while (parse.HasMoreCommands()) {
                parse.Advance();

                string binaryCode = String.Empty;

                if (parse.CommandType() == typeOfCommand.A_COMMAND) {
                    binaryCode = parse.Symbol();
                    bool isNumeric = int.TryParse(binaryCode, out int n);

                    if (isNumeric) {
                        binaryCode = translateFromNumeric(n);
                    } else {
                        if (table.Contains(binaryCode)) n = table.GetAddress(binaryCode);
                        else {
                            table.addEntry(binaryCode, ramAddress);
                            n = ramAddress;
                            ramAddress++;
                        }
                        binaryCode = translateFromNumeric(n);
                    }
                } else if (parse.CommandType() == typeOfCommand.C_COMMAND) {
                    binaryCode = "111";
                    string comp = parse.Comp();
                    string dest = parse.Dest();
                    string jump = parse.Jump();
                    binaryCode += Code.Comp(comp) + Code.Dest(dest) + Code.Jump(jump);
                }
                if (!binaryCode.Equals(string.Empty))
                    hackWriter.WriteLine(binaryCode);
            }

            hackWriter.Close();
            hackFile.Close();
            parse.Close();
        }
    }
}
