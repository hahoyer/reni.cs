using System;
using NUnit.Framework;

namespace Reni.FeatureTest
{
    [TestFixture]
    public class SimpleFiles : CompilerTest
    {
        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// created 10.01.2007 04:32
        [SetUp]
        public new void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test,Category(Worked)]
        public void IntegerClass()
        {
            RunCompiler("Integer8"
                , @"
Integer8: function
{
    127 type (arg);
    dump_print: (_A_T_ 0) dump_print;
    + : function Integer8(_A_T_0 + arg)
};

Integer8(128) dump_print;
(Integer8(1)+Integer8(2)) dump_print;
"
                , "-128"
                );
        }
    }
}
