using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace Reni
{
    public sealed class T4Compiler
    {
        readonly string _text;
        readonly string ModuleName;

        public T4Compiler(string text, string moduleName = "Reni")
        {
            _text = text;
            ModuleName = moduleName;
        }

        public string Code()
        {
            var fileName = Environment.GetEnvironmentVariable("temp") + "\\reni\\T4Compiler.reni";
            var f = fileName.FileHandle();
            f.AssumeDirectoryOfFileExists();
            f.String = _text;
            var compiler = new Compiler(fileName, moduleName: ModuleName);
            return compiler.CSharpString;
        }
    }
}