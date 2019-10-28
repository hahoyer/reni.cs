using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniTest
{
    [UnitTest]
    [TestFixture]
    sealed class FilebasedTests
    {
        [UnitTest]
        [Test]
        public void TestMethod()
        {
            var fileName = Extension.SolutionDir + @"\renisource\tests";
            Run(fileName.ToSmbFile());
        }

        static void Run(SmbFile file)
        {
            Tracer.Line(Tracer.FilePosn(file.FullName,0,0,0,0,FilePositionTag.Test));
            if(file.IsDirectory)
                foreach(var item in file.Items)
                    Run(item);
            else if(file.Exists && file.Extension == "renitest")
                Run(new Source(file.FullName.ToSmbFile()));
        }

        static void Run(Source file)
        {
             new FileTestCompiler(file).Run();
        }
    }
}