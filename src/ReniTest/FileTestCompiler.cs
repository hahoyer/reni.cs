using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace ReniTest
{
    sealed class FileTestCompiler : DumpableObject
    {
        internal FileTestCompiler(Source file) { }

        public void Run() { NotImplementedMethod(); }
    }

    abstract class TreeItem : DumpableObject, ISourcePart
    {
        SourcePart ISourcePart.All
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }

}