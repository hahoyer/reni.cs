using System;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using Reni;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Integer;
using Reni.FeatureTest.Struct;
using Reni.FeatureTest.BitArrayOp;

namespace ReniTest
{
    public static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TreeForm { Target = CreateCompiler(new DumpPrint1().Target) });
        }

        private static Compiler CreateCompiler(string text)
        {
            var fileName = "temptest.reni";
            var f = File.m(fileName);
            f.String = text;
            return new Compiler(fileName);
            //_compiler.Exec();
        }
    }
}