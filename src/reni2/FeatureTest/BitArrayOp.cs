using NUnit.Framework;

namespace Reni.FeatureTest.BitArrayOp 
{
    /// <summary>
    /// Operations on bitarrays
    /// </summary>
    [TestFixture]
    public class BitArrayOp : CompilerTest
    {
        [Test]
        public void PositiveNumbers2()
        {
            RunCompiler("Numbers",@"(1, 12)dump_print","(1, 12)");
        }

        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void PositiveNumbers()
        {
            RunCompiler("Numbers of different Size", 
            @"(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)dump_print", 
            "(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)");
        }

        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void NegativeNumber()
        {
            RunCompiler("Negative number", @"(-1)dump_print", "-1");
            RunCompiler("Negative number", @"(-12)dump_print", "-12");
            RunCompiler("Negative number", @"(-123)dump_print", "-123");
            RunCompiler("Negative number", @"(-1234)dump_print", "-1234");
            RunCompiler("Negative number", @"(-12345)dump_print", "-12345");
            RunCompiler("Negative number", @"(-123456)dump_print", "-123456");
            RunCompiler("Negative number", @"(-1234567)dump_print", "-1234567");
            RunCompiler("Negative number", @"(-12345678)dump_print", "-12345678");
            RunCompiler("Negative number", @"(-123456789)dump_print", "-123456789");
            RunCompiler("Negative number", @"(-1234567890)dump_print", "-1234567890");
        }
        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void NegativeNumbers2()
        {
            RunCompiler("Numbers of different Size",
            @"(-1, -12)dump_print",
            "(-1, -12)");
        }
        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void NegativeNumbers()
        {
            RunCompiler("Numbers of different Size",
            @"(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)dump_print",
            "(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)");
        }
        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void AddOddSizedNumber()
        {
            RunCompiler("1st", @"(40000 - 1  )dump_print", "39999");
            RunCompiler("1st", @"(40000 + 1   )dump_print", "40001");
            RunCompiler("1st", @"(40000 - 43210)dump_print", "-3210");
            RunCompiler("1st", @"( 400 - 43210)dump_print", "-42810");
        }

        public override void Run()
        {
        }

    }

    [TestFixture]
    public class Add2Numbers : CompilerTest
    {
        public override string Target { get { return @"(2+4) dump_print"; } }
        public override string Output { get { return "6"; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

}