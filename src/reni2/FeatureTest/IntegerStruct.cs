using NUnit.Framework;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Structure, that is all between brackets
    /// </summary>
    [TestFixture]
    public class IntegerStruct : CompilerTest
    {
        static string IntegerDefinition()
        {
            return @"
Integer8: function
{
    127 type (arg);
    dump_print: property ((_A_T_ 0) dump_print);
    + : function Integer8(_A_T_ 0 + arg)
}
";
        }

        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Category(Worked)]
        public void DumpPrint()
        {
            RunCompiler("DumpPrint"
                        , IntegerDefinition() + "; Integer8(128) dump_print"
                        , "-128"
                );
        }
        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Category(Worked)]
        public void Plus()
        {
            RunCompiler("Plus"
                        , IntegerDefinition() + "; (Integer8(1)+Integer8(2)) dump_print"
                        , "3"
                );
        }
    }
}