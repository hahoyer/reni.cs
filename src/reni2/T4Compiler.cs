using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace Reni
{
    public sealed class T4Compiler
    {
        readonly string _text;

        public T4Compiler(string text) { _text = text; }

        public string Code()
        {
            var fileName = Environment.GetEnvironmentVariable("temp") + "\\reni\\T4Compiler.reni";
            var f = fileName.FileHandle();
            f.AssumeDirectoryOfFileExists();
            f.String = _text;
            var compiler = Compiler.FromFile(fileName, "T4Compiler");
            return compiler.CSharpString;
        }
    }
}