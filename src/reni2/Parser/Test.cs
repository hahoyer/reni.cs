using NUnit.Framework;
using Reni.FeatureTest;

namespace Reni.Parser
{
    [TestFixture]
    public class Test : CompilerTest
    {
        [SetUp]
        public new void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Special test, will not work automatically.
        /// </summary>
        /// created 18.07.2007 01:27 on HAHOYER-DELL by hh
        [Test, Explicit]
        public void SimpleFunction()
        {
            Parameters.Trace.Source = true;
            RunCompiler("SimpleFunction", @"f()", "3");
        }

    }
}
