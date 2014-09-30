using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureTest.BitArrayOp
{
    /// <summary>
    ///     Operations on bitarrays
    /// </summary>
    [TestFixture]
    [Number]
    public sealed class BitArrayOp : CompilerTest
    {
        /// <summary>
        ///     Compares the operators.
        /// </summary>
        /// created 08.01.2007 00:05
        [Test]
        public void NegativeNumber()
        {
            CreateFileAndRunCompiler("Negative number", @"(0-1)dump_print", "-1");
            CreateFileAndRunCompiler("Negative number", @"(0-12)dump_print", "-12");
            CreateFileAndRunCompiler("Negative number", @"(0-123)dump_print", "-123");
            CreateFileAndRunCompiler("Negative number", @"(0-1234)dump_print", "-1234");
            CreateFileAndRunCompiler("Negative number", @"(0-12345)dump_print", "-12345");
            CreateFileAndRunCompiler("Negative number", @"(0-123456)dump_print", "-123456");
            CreateFileAndRunCompiler("Negative number", @"(0-1234567)dump_print", "-1234567");
            CreateFileAndRunCompiler("Negative number", @"(0-12345678)dump_print", "-12345678");
            CreateFileAndRunCompiler("Negative number", @"(0-123456789)dump_print", "-123456789");
            CreateFileAndRunCompiler("Negative number", @"(0-1234567890)dump_print", "-1234567890");
        }

        public override void Run() { }
    }

    [TestFixture]
    [Target(@"(1, 12)dump_print")]
    [Output("(1, 12)")]
    [Number]
    public sealed class TwoPositiveNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TwoPositiveNumbers]
    [Target(@"(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)dump_print")]
    [Output("(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)")]
    public sealed class PositiveNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TwoPositiveNumbers]
    [Target(@"(-1, -12)dump_print")]
    [Output("(-1, -12)")]
    public sealed class TwoNegativeNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TwoNegativeNumbers]
    [Target(@"(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)dump_print")]
    [Output("(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)")]
    public sealed class NegativeNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"3 dump_print")]
    [Output("3")]
    public sealed class Number : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"(2+4) dump_print")]
    [Output("6")]
    [Number]
    public sealed class Add2Numbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"(40000 - 1  )dump_print")]
    [Output("39999")]
    [Number]
    public sealed class SubtractOddSizedNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"(40000 + 1  )dump_print")]
    [Output("40001")]
    [Number]
    public sealed class AddOddSizedNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"(40000 - 43210)dump_print")]
    [Output("-3210")]
    [Number]
    public sealed class SubtractLargeEqualSizedNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"(400 - 43210)dump_print")]
    [Output("-42810")]
    [Number]
    public sealed class SubtractLargerSizedNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}