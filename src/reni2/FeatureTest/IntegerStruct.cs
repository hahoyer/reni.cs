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

    create   : function(Integer8(arg));
    dump_print: property function (_data dump_print);
    +        : function create(_data + create(arg) _data);
    clone    : function create(_data);
    converter _data
}
";
        }

        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Category(Worked)]
        public void DumpPrint1()
        {
            RunCompiler("DumpPrint1"
                        , IntegerDefinition() + "; Integer8(1) dump_print"
                        , "1"
                );
        }
        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Category(Worked)]
        public void DumpPrint2()
        {
            Parameters.Trace.All();
            RunCompiler("DumpPrint2"
                        , IntegerDefinition() + "; Integer8(2) dump_print"
                        , "2"
                );
        }
        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Category(Worked)]
        public void DumpPrint127()
        {
            RunCompiler("DumpPrint127"
                        , IntegerDefinition() + "; Integer8(127) dump_print"
                        , "127"
                );
        }
        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Explicit, Category(UnderConstruction)]
        public void Create()
        {
            RunCompiler("Create"
                        , IntegerDefinition() + "; Integer8(0) create(23) dump_print"
                        , "23"
                );
        }
        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Explicit, Category(UnderConstruction)]
        public void Clone()
        {
            RunCompiler("Clone"
                        , IntegerDefinition() + "; Integer8(23) clone() dump_print"
                        , "23"
                );
        }

        /// <summary>
        /// Integers the class.
        /// </summary>
        [Test, Explicit, Category(UnderConstruction)]
        public void Plus()
        {
            RunCompiler("Plus", PlusText, "3");
        }

        public static string PlusText
        {
             get
             {
                  return IntegerDefinition() 
                      + "; (Integer8(1)+Integer8(2)) dump_print";
             }
        }
        public override void Run() {  }
    }
}