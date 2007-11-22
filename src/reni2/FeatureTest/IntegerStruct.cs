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
    _data: 127 type (arg enable_cut);

    create   : function(Interger8(arg));
    dump_print: property function (_data dump_print);
    +        : function create(_data + create(arg) _data);
    converter _data
}
";
        }

        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Explicit, Category(Worked)]
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
        [Test, Explicit, Category(UnderConstruction)]
        public void Plus()
        {
            RunCompiler("Plus"
                        , IntegerDefinition() + "; (Integer8(1)+Integer8(2)) dump_print"
                        , "3"
                );
        }
    }
}