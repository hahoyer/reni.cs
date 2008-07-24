using NUnit.Framework;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Conditional expression
    /// </summary>
    [TestFixture]
    public class ThenElse: CompilerTest
    {
        /// <summary>
        /// Simple condition.
        /// </summary>
        /// created 05.01.2007 02:13
        [Test]
        [Category(UnderConstruction)]
        public void Simple()
        {
            RunCompiler("Simple", @"x: 1=0 then 1 else 100;x dump_print;", "100");
            RunCompiler("Simple", @"x: 1=1 then 1 else 100;x dump_print;", "1");
        }

        public override void Run() {  }
    }
}
