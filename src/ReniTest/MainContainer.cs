//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using Reni;
using Reni.FeatureTest.DefaultOperations;

namespace ReniTest
{
    internal static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //TestGenerated.Exec();

            //CompilerTest.Run("Test", "(-1234) dump_print", "-1234");
            //RunSpecificTest();
            if(Debugger.IsAttached)
                TestRunner.IsModeErrorFocus = true;
            Assembly.GetExecutingAssembly().RunTests();
            //InspectCompiler();
            //Reni.Proof.Main.Run();
        }

        [Test]
        private static void RunSpecificTest() { new TypeNameExtenderTest().TestMethod(); }

        private const string Target = @"! property x: 11/\; x dump_print";
        private const string Output = "11";
        private static void InspectCompiler() { Application.Run(new TreeForm {Target = CreateCompiler(Target)}); }

        private static Compiler CreateCompiler(string text)
        {
            const string fileName = "temptest.reni";
            var f = File.m(fileName);
            f.String = text;
            var compiler = new Compiler(fileName);
            //Profiler.Measure(()=>compiler.Exec());
            //Tracer.FlaggedLine(Profiler.Format(10,0.0));
            return compiler;
        }
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
            InternalTest(typeof(TypeOperator), "DefaultOperations.TypeOperator");
        }

        [DebuggerHidden]
        private static void InternalTest(Type type, string expectedTypeName) { Tracer.Assert(1, type.PrettyName() == expectedTypeName, () => type + "\nFound   : " + type.PrettyName() + "\nExpected: " + expectedTypeName); }
    }
}