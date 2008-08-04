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
            CreateFileAndRunCompiler("Numbers",@"(1, 12)dump_print","(1, 12)");
        }

        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void PositiveNumbers()
        {
            CreateFileAndRunCompiler("Numbers of different Size", 
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
            CreateFileAndRunCompiler("Negative number", @"(-1)dump_print", "-1");
            CreateFileAndRunCompiler("Negative number", @"(-12)dump_print", "-12");
            CreateFileAndRunCompiler("Negative number", @"(-123)dump_print", "-123");
            CreateFileAndRunCompiler("Negative number", @"(-1234)dump_print", "-1234");
            CreateFileAndRunCompiler("Negative number", @"(-12345)dump_print", "-12345");
            CreateFileAndRunCompiler("Negative number", @"(-123456)dump_print", "-123456");
            CreateFileAndRunCompiler("Negative number", @"(-1234567)dump_print", "-1234567");
            CreateFileAndRunCompiler("Negative number", @"(-12345678)dump_print", "-12345678");
            CreateFileAndRunCompiler("Negative number", @"(-123456789)dump_print", "-123456789");
            CreateFileAndRunCompiler("Negative number", @"(-1234567890)dump_print", "-1234567890");
        }
        /// <summary>
        /// Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test, Category(Worked)]
        public void NegativeNumbers2()
        {
            CreateFileAndRunCompiler("Numbers of different Size",
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
            CreateFileAndRunCompiler("Numbers of different Size",
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
            CreateFileAndRunCompiler("1st", @"(40000 - 1  )dump_print", "39999");
            CreateFileAndRunCompiler("1st", @"(40000 + 1   )dump_print", "40001");
            CreateFileAndRunCompiler("1st", @"(40000 - 43210)dump_print", "-3210");
            CreateFileAndRunCompiler("1st", @"( 400 - 43210)dump_print", "-42810");
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