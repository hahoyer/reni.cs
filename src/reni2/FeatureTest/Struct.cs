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
    [TargetSet(@"(1, 11, 3, (this _A_T_ 1) := 3) dump_print","(1, 3, 3, )")]
    public class Assignment : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, InnerAccess]
    [TargetSet("5, (this _A_T_ 0 + this _A_T_ 0)dump_print","10")]
    [TargetSet(" 1; 4;2050; (this _A_T_ 0) + (this _A_T_ 1) + (this _A_T_ 2);(this _A_T_ 3) dump_print;", "2055")]
    public class AccessAndAdd : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("5, (this _A_T_ 0) dump_print", "5")]
    [TargetSet("5,6, (this _A_T_ 0) dump_print", "5")]
    [TargetSet("5,6, (this _A_T_ 1) dump_print", "6")]
    public class InnerAccess : CompilerTest
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
    [AccessAndAdd]
    public class SomeVariables : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"a: (b:222,c:4); a b dump_print","222")]
    [TargetSet(@"a: (b:222,c:4); a c dump_print", "4")]
    [InnerAccess]
    public class AccessMember : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}