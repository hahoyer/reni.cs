using NUnit.Framework;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.DefaultOperations
{
    /// <summary>
    /// Default operators
    /// </summary>
    [TestFixture]
    public class DefaultOperations: CompilerTest
    {
        public override string Target { get { return @"x: 0; x type dump_print"; } }
        public override string Output { get { return "(bit)sequence(1)"; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(SomeVariables) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
    /// <summary>
    /// Default operators
    /// </summary>
    [TestFixture]
    public class OldStyle : CompilerTest
    {
        [Test, Category(Worked)]
        public void TypeOperator()
        {
            RunCompiler("TypeOperator", @"31 type dump_print", "(bit)sequence(6)");
        }
        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void CompareOperators()
        {
            RunCompiler("CompareOperator", @"(1=100)dump_print", "0");
            RunCompiler("CompareOperator", @"(1<>100)dump_print", "-1");
            RunCompiler("CompareOperator", @"(1>100)dump_print", "0");
            RunCompiler("CompareOperator", @"(1<100)dump_print", "-1");
            RunCompiler("CompareOperator", @"(1<=100)dump_print", "-1");
            RunCompiler("CompareOperator", @"(1>=100)dump_print", "0");
        }

        /// <summary>
        /// Apply type operator.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void ApplyTypeOperator()
        {
            RunCompiler("ApplyTypeOperator", @"(31 type (28))dump_print", "28");
        }
        /// <summary>
        /// Applies the type operator with cut.
        /// </summary>
        /// created 19.08.2007 21:06 on HAHOYER-DELL by hh
        [Test, Category(Worked)]
        public void ApplyTypeOperatorWithCut()
        {
            RunCompiler("ApplyTypeOperator", @"(31 type (100 enable_cut))dump_print", "-28");
        }

        public override void Run()
        {
        }
    }
}

