using hw.DebugFormatter;
using hw.UnitTest;
using Reni.Basics;
using Reni.Context;

namespace Reni.FeatureTest.ConversionService;

[UnitTest]
public sealed class NumberPointerCutConversion : DependenceProvider
{
    [UnitTest]
    public void Run()
    {
        var source = new Root(null).BitType.Number(8).EnableCut.Pointer;
        var destination = new Root(null).BitType.Number(6);
        var path = Type.ConversionService.FindPath(source, destination);

        (path != null).Assert();

        var calculatedDestination = path.Execute(Category.Type).Type;
        (calculatedDestination == destination).Assert();

        var code = path.Execute(Category.Code | Category.Type).Code
            .ReplaceArg(source.ArgResult(Category.Code | Category.Type));
        code.AssertIsNotNull();
    }
}