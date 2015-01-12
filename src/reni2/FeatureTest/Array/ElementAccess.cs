using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [ArrayFromPieces]
    [TargetSet("((<<5) (0)) dump_print", "5")]
    public sealed class ElementAccessSimple : CompilerTest {}

    [TestFixture]
    [ElementAccessSimple]
    [TargetSet("((<<5<<3<<5<<1<<3) (3)) dump_print", "1")]
    public sealed class ElementAccess : CompilerTest {}

    [TestFixture]
    [ArrayFromPieces]
    [TwoStatements]
    [TargetSet("x: <<5<<3; x dump_print", "<<(5, 3)")]
    public sealed class ArrayVariable : CompilerTest {}

    [TestFixture]
    [ElementAccessVariable]
    [ArrayVariable]
    [TwoStatements]
    [TargetSet("x:  <<:= 5; x(0) := 2; x dump_print", "<<:=(2)")]
    public sealed class ElementAccessVariableSetterSimple : CompilerTest {}

    [TestFixture]
    [ElementAccessVariableSetterSimple]
    [TwoStatements]
    [TargetSet("x:  <<:=5<<3<<5<<1<<3; x(3) := 2; x dump_print", "<<:=(5, 3, 5, 2, 3)")]
    public sealed class ElementAccessVariableSetter : CompilerTest {}

    [TestFixture]
    [ElementAccess]
    [TwoStatements]
    [TargetSet("x: <<5<<3<<5<<1<<3; x(3) dump_print", "1")]
    public sealed class ElementAccessVariable : CompilerTest {}
}