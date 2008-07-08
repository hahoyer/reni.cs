using NUnit.Framework;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Structure, that is all between brackets
    /// </summary>
    [TestFixture]
    public class Struct : CompilerTest
    {
        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test]
        [Category(Worked)]
        public void AccessSimple()
        {
            RunCompiler("AccessSimple", @"((0, 1) _A_T_ 0) dump_print;", "0");
        }

        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test]
        [Category(Worked)]
        public void AccessSimpleAdditionalColon()
        {
            RunCompiler("AccessSimpleAdditionalColon", @"((0, 1,) _A_T_ 0) dump_print;", "0");
        }

        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test]
        [Category(Worked)]
        public void StrangeStructs()
        {
            RunCompiler("AccessVarAdditionalColon", @"((one: 1) one) dump_print;", "1");
            RunCompiler("AccessVarAdditionalColon", @"((one: 1,) one) dump_print;", "1");
            RunCompiler("AccessVarAdditionalColon", @"((0,one: 1) one) dump_print;", "1");
            RunCompiler("AccessVarAdditionalColon", @"((0,one: 1,) one) dump_print;", "1");
        }

        /// <summary>
        /// Access to elements of a structure.
        /// </summary>
        /// created 17.11.2006 20:43
        [Test]
        [Category(Worked)]
        public void Access()
        {
            RunCompiler("Access",
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
        /// Access to elementas of a structure inside the structure.
        /// </summary>
        /// created 17.11.2006 20:44
        [Test]
        [Category(Worked)]
        public void AccessEx()
        {
            RunCompiler("SomeVariables",
                        @"
 1;
 4;
2050;
 (this _A_T_ 0) + (this _A_T_ 1) + (this _A_T_ 2);
(this _A_T_ 3) dump_print;
"
                        , "2055"
                );
        }

        /// <summary>
        /// Declaration and access to variables
        /// </summary>
        /// created 17.11.2006 20:44
        [Test]
        [Category(Worked)]
        public void SomeVariables()
        {
            RunCompiler("SomeVariables",
                        @"
x1: 1;
x2: 4;
x3: 2050;
x4: x1 + x2 + x3;
x4 dump_print;
"
                        , "2055"
                );
        }

        /// <summary>
        /// Declaration and access to variables
        /// </summary>
        /// created 17.11.2006 20:44
        [Test, Category(Worked)]
        public void DumpPrint()
        {
            RunCompiler("DumpPrint",
                        @"(1, 2, 3, 4, 5, 6) dump_print",
                        "(1, 2, 3, 4, 5, 6)"
                );
        }

        /// <summary>
        /// Declaration and access to variables
        /// </summary>
        /// created 17.11.2006 20:44
        [Test, Category(Worked)]
        public void Assignment()
        {
            RunCompiler("Assignment",
                        @"(1, 11, 3, (this _A_T_ 1) := 3) dump_print",
                        "(1, 3, 3, )"
                );
        }

        /// <summary>
        /// Declaration and access to properties
        /// </summary>
        /// created 17.11.2006 20:44
        [Test, Category(Worked)]
        public void PropertyVariable()
        {
            RunCompiler("PropertyVariable",
                        @"x: property function 11; x dump_print",
                        "11"
                );
        }

    }
}
