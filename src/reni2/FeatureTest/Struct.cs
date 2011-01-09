using HWClassLibrary.UnitTest;
using System;
using System.Collections.Generic;

namespace Reni.FeatureTest.Struct
{
    /// <summary>
    /// Structure, that is all between brackets
    /// </summary>
    [TestFixture]
    public class OldStyle : CompilerTest
    {
        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test, Category(Worked)]
        public void AccessSimple() { CreateFileAndRunCompiler("AccessSimple", @"((0, 1) _A_T_ 0) dump_print;", "0"); }

        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test, Category(Worked)]
        public void AccessSimpleAdditionalColon() { CreateFileAndRunCompiler("AccessSimpleAdditionalColon", @"((0, 1,) _A_T_ 0) dump_print;", "0"); }

        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test, Category(Worked)]
        public void StrangeStructs()
        {
            CreateFileAndRunCompiler("AccessVarAdditionalColon", @"((one: 1) one) dump_print;", "1");
            CreateFileAndRunCompiler("AccessVarAdditionalColon", @"((one: 1,) one) dump_print;", "1");
            CreateFileAndRunCompiler("AccessVarAdditionalColon", @"((0,one: 1) one) dump_print;", "1");
            CreateFileAndRunCompiler("AccessVarAdditionalColon", @"((0,one: 1,) one) dump_print;", "1");
        }

        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test, Category(Worked)]
        public void Access()
        {
            CreateFileAndRunCompiler("Access",
                @"
((0;1;2;300;) _A_T_ 0) dump_print;
((0;1;2;300;) _A_T_ 1) dump_print;
((0;1;2;300;) _A_T_ 2) dump_print;
((0;1;2;300;) _A_T_ 3) dump_print;
"
                , "012300"
                );
        }

        /// <summary>
        /// Declaration and access to variables
        /// </summary>
        /// created 17.11.2006 20:44
        [Test, Category(Worked)]
        public void DumpPrint()
        {
            CreateFileAndRunCompiler("DumpPrint",
                @"(1, 2, 3, 4, 5, 6) dump_print",
                "(1, 2, 3, 4, 5, 6)"
                );
        }

        public override void Run() { }
    }

    [TestFixture, InnerAccess]
    public class PropertyVariable: CompilerTest{
        public override string Target { get { return @"! property x: 11/\; x dump_print"; } }
        protected override string Output { get { return "11"; } }
    
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, InnerAccess]
    [TargetSet(@"(10, (this _A_T_ 0) := 4) dump_print", "(4, )")]
    [TargetSet(@"(10,20, (this _A_T_ 0) := 4) dump_print", "(4, 20, )")]
    [TargetSet(@"(10,20,30, (this _A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
    [TargetSet(@"(10,20, (this _A_T_ 1) := 4) dump_print", "(10, 4, )")]
    [TargetSet(@"(10,20,30, (this _A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [TargetSet(@"(10,20,30, (this _A_T_ 2) := 4) dump_print", "(10, 20, 4, )")]
    [TargetSet(@"(1000,20,30, (this _A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
    [TargetSet(@"(1000,20,30, (this _A_T_ 1) := 4) dump_print", "(1000, 4, 30, )")]
    [TargetSet(@"(10,2000,30, (this _A_T_ 0) := 4) dump_print", "(4, 2000, 30, )")]
    [TargetSet(@"(10,2000,30, (this _A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [TargetSet(@"(10,2000,30, (this _A_T_ 2) := 4) dump_print", "(10, 2000, 4, )")]
    [TargetSet(@"(3, (this _A_T_ 0) := 5 enable_cut) dump_print", "(-3, )")]
    public class Assignment : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [AccessAndAdd]
    [TargetSet(" 1; 4;2050; (this _A_T_ 0) + (this _A_T_ 1) + (this _A_T_ 2);(this _A_T_ 3) dump_print;", "2055")]
    public sealed class AccessAndAddComplex : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [TargetSet("5, (this _A_T_ 0 + this _A_T_ 0)dump_print", "10")]
    public sealed class AccessAndAdd : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, BitArrayOp.Number]
    [TargetSet("5, (this _A_T_ 0) dump_print, 66", "5")]
    [TargetSet("5,6, (this _A_T_ 0) dump_print, 66", "5")]
    [TargetSet("5,6, (this _A_T_ 1) dump_print, 66", "6")]
    public sealed class InnerAccess : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"
x1: 1;
x2: 4;
x3: 2050;
x4: x1 + x2 + x3;
x4 dump_print;
","2055")]
    [AccessAndAddComplex]
    public sealed class SomeVariables : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a b dump_print","222")]
    [TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a c dump_print", "4")]
    [InnerAccess]
    public class AccessMember : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}