using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

// ReSharper disable once CheckNamespace
namespace Reni.FeatureTest.BitArrayOp;

/// <summary>
///     Operations on bitarrays
/// </summary>
[UnitTest]
[Number]
[Negate]
public sealed class BitArrayOp : CompilerTest
{
    /// <summary>
    ///     Compares the operators.
    /// </summary>
    /// created 08.01.2007 00:05
    [UnitTest]
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
}

[UnitTest]
[Target(@"(1 negate)dump_print")]
[Output("-1")]
[Number]
[ConversionService.Closure]
public sealed class Negate : CompilerTest;

[UnitTest]
[Target(@"(1, 12)dump_print")]
[Output("(1, 12)")]
[Number]
[ConversionService.Closure]
public sealed class TwoPositiveNumbers : CompilerTest;

[UnitTest]
[TwoPositiveNumbers]
[Target(@"(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)dump_print")]
[Output("(1, 12, 123, 1234, 12345, 123456, 1234567, 12345678, 123456789, 1234567890)")]
public sealed class PositiveNumbers : CompilerTest;

[UnitTest]
[TwoPositiveNumbers]
[Target(@"(-1, -12)dump_print")]
[Output("(-1, -12)")]
public sealed class TwoNegativeNumbers : CompilerTest;

[UnitTest]
[TwoNegativeNumbers]
[Target(@"(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)dump_print")]
[Output("(-1, -12, -123, -1234, -12345, -123456, -1234567, -12345678, -123456789, -1234567890)")]
public sealed class NegativeNumbers : CompilerTest;

[UnitTest]
[Target(@"3 dump_print")]
[Output("3")]
[DumpPrint]
public sealed class Number : CompilerTest;

[UnitTest]
[Target(@"(2+4) dump_print")]
[Output("6")]
[Number]
[ConversionService.Closure]
public sealed class Add2Numbers : CompilerTest;

[UnitTest]
[Target(@"(40000 - 1  )dump_print")]
[Output("39999")]
[Number]
[ConversionService.Closure]
public sealed class SubtractOddSizedNumber : CompilerTest;

[UnitTest]
[Target(@"(40000 + 1  )dump_print")]
[Output("40001")]
[Number]
[ConversionService.Closure]
public sealed class AddOddSizedNumber : CompilerTest;

[UnitTest]
[Target(@"(40000 - 43210)dump_print")]
[Output("-3210")]
[Number]
[ConversionService.Closure]
public sealed class SubtractLargeEqualSizedNumber : CompilerTest;

[UnitTest]
[Target(@"(400 - 43210)dump_print")]
[Output("-42810")]
[Number]
[ConversionService.Closure]
public sealed class SubtractLargerSizedNumber : CompilerTest;