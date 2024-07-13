using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.Validation;

namespace Reni.FeatureTest;

[UnitTest]
[SimpleAssignment]
[FunctionOfFunction]
[UndefinedContextSymbol]
[UndefinedSymbol]
[UseOfUndefinedContextSymbol]
[IndirectUseOfUndefinedContextSymbol]
[Annotated]
[AllScopeHandling]
[ConcatDeclaration]
[Output("Ha")]
public sealed class ComplexContext : CompilerTest
{
    protected override string Target => (SmbFile.SourceFolder / "ComplexContext.reni").String;
}