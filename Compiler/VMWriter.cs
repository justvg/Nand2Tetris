using System.IO;

namespace Compiler {

    /* Emits VM commands into a file, using the VM command syntax
     */
    class VMWriter {
        FileStream vmFile;
        StreamWriter vmWrtr;

        public VMWriter() { }

        public VMWriter(string path) {
            vmFile = new FileStream(path, FileMode.Create);
            vmWrtr = new StreamWriter(vmFile);
        }

        public void WritePush(string segment, int index) {
            vmWrtr.WriteLine("push " + segment + " " + index);
        }

        public void WritePop(string segment, int index) {
            vmWrtr.WriteLine("pop " + segment + " " + index);
        }

        public void WriteArithmetic(string command) {
            switch (command) {
                case "*": vmWrtr.WriteLine("call Math.multiply 2"); break;
                case "/": vmWrtr.WriteLine("call Math.divide 2"); break;
                case "+": vmWrtr.WriteLine("add"); break;
                case "-": vmWrtr.WriteLine("sub"); break;
                case "neg": vmWrtr.WriteLine("neg"); break;
                case "~": vmWrtr.WriteLine("not"); break;
                case "=": vmWrtr.WriteLine("eq"); break;
                case "&amp;": vmWrtr.WriteLine("and"); break;
                case "|": vmWrtr.WriteLine("or"); break;
                case "&lt;": vmWrtr.WriteLine("lt"); break;
                case "&gt;": vmWrtr.WriteLine("gt"); break;
            }
        }

        public void WriteLabel(string label) {
            vmWrtr.WriteLine("label " + label);
        }

        public void WriteGoto(string label) {
            vmWrtr.WriteLine("goto " + label);
        }

        public void WriteIf(string label) {
            vmWrtr.WriteLine("if-goto " + label);
        }

        public void WriteCall(string name, int nArgs) {
            vmWrtr.WriteLine("call " + name + " " + nArgs);
        }

        public void WriteFunction(string name, int nLocals) {
            vmWrtr.WriteLine("function " + name + " " + nLocals);
        }

        public void WriteReturn() {
            vmWrtr.WriteLine("return");
        }

        public void Close() {
            vmWrtr.Close();
            vmFile.Close();       
        }
    }
}
