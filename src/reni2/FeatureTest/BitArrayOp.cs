using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.BitArrayOp
{
    /// <summary>
    ///     Operations on bitarrays
    /// </summary>
    [TestFixture, Number]
    public class BitArrayOp : CompilerTest
    {
        /// <summary>
        ///     Compares the operators.
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

        public override void Run() { }
    }

    [TestFixture,
     Target(@"(1, 12)dump_print"),
     Output("(1, 12)"), Number]
    public class TwoPositiveNumbers : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, TwoPositiveNumbers,
     Target(@"(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)dump_print"),
     Output("(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)")]
    public class PositiveNumbers : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, TwoPositiveNumbers,
     Target(@"(-1, -12)dump_print"),
     Output("(-1, -12)")]
    public class TwoNegativeNumbers : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, TwoNegativeNumbers,
     Target(@"(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)dump_print"),
     Output("(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)")]
    public class NegativeNumbers : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"3 dump_print"), Output("3")]
    public class Number : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"(2+4) dump_print"), Output("6"), Number]
    public class Add2Numbers : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"(40000 - 1  )dump_print"), Output("39999"), Number]
    public class SubtractOddSizedNumber : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"(40000 + 1  )dump_print"), Output("40001"), Number]
    public class AddOddSizedNumber : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"(40000 - 43210)dump_print"), Output("-3210"), Number]
    public class SubtractLargeEqualSizedNumber : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"(400 - 43210)dump_print"), Output("-42810"), Number]
    public class SubtractLargerSizedNumber : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}