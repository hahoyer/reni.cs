using System;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Integer;
using Reni.FeatureTest.Struct;
using Reni.FeatureTest.BitArrayOp;
using File=HWClassLibrary.IO.File;

namespace ReniTest
{
    public static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TreeForm { Target = CreateCompiler(new Plus().Target) });
        }

        private static Compiler CreateCompiler(string text)
        {
            const string fileName = "temptest.reni";
            var f = File.m(fileName);
            f.String = text;
            return new Compiler(fileName);
            //_compiler.Exec();
        }
    }
}