#region Copyright (C) 2012

//     Project reniTest
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using Reni;
using Reni.FeatureTest.TypeType;
using Reni.Parser;
using Reni.Runtime;

namespace ReniTest
{
    static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ShowSyntaxTree();
            return;
            //if(false)
            ExecTest();

            if(Debugger.IsAttached)
                TestRunner.IsModeErrorFocus = true;
            Assembly.GetExecutingAssembly().RunTests();
            //InspectCompiler();
            //Reni.Proof.Main.Run();
        }

        const string Target = @"f: /\arg(); x: 1; f(/\x) dump_print";
        const string Output = "1";
        static void InspectCompiler() { Application.Run(new TreeForm {Target = CreateCompiler(Target)}); }
        static void ShowSyntaxTree()
        {
            var prioTable = @"Left not
Left and
Left or
Left * /
Left + -
Left = <>
Right :=
TELevel then else
Left function
Right :
Right , ;
ParLevel ( { ) }
".FormatPrioTable();
            var image = prioTable.SyntaxGraph("a <> b then (a :=b) else (x(); y(a,d))");
            var mainForm = new Form
            {
                ClientSize = image.Size, 
                BackgroundImage = image, 
                BackgroundImageLayout = ImageLayout.Stretch
            };

            Application.Run(mainForm);
        }

        static Compiler CreateCompiler(string text)
        {
            Tracer.IsBreakDisabled = false;
            const string fileName = "temptest.reni";
            var f = fileName.FileHandle();
            f.String = text;
            var compiler = new Compiler(fileName);
            //Profiler.Measure(()=>compiler.Exec());
            //Tracer.FlaggedLine(Profiler.Format(10,0.0));
            return compiler;
        }

        static void ExecTest()
        {
            Data.OutStream = new OutStream();
            Reni.MainFunction();
            Data.OutStream = null;
        }

        [TestFixture]
        public sealed class TypeNameExtenderTest
        {
            [Test]
            public void TestMethod()
            {
                InternalTest(typeof(int), "int");
                InternalTest(typeof(List<int>), "List<int>");
                InternalTest(typeof(List<List<int>>), "List<List<int>>");
                InternalTest(typeof(Dictionary<int, string>), "Dictionary<int,string>");
                InternalTest(typeof(TypeOperator), "TypeType.TypeOperator");
            }

            [DebuggerHidden]
            static void InternalTest(Type type, string expectedTypeName) { Tracer.Assert(1, type.PrettyName() == expectedTypeName, () => type + "\nFound   : " + type.PrettyName() + "\nExpected: " + expectedTypeName); }
        }
    }
}