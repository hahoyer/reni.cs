using hw.UnitTest;
using Reni.Context;
using Reni.Type;

namespace Reni.FeatureTest.ConversionService;

[UnitTest]
public sealed class Closure : DependenceProvider
{
    [UnitTest]
    public void OfNumber()
    {
        var number = new Root(null!).BitType.Number(1);
        var types = number.SymmetricFeatureClosure().Types().ToArray();

        types.Contains(number).Assert();
        types.Contains(number.Align).Assert();
        types.Contains(number.Pointer).Assert();
        (types.Length == 3).Assert(() => "\n" + types.Stringify("\n"));
    }

    [UnitTest]
    public void ClosureServiceOfAlignedNumber()
    {
        var source = new Root(null!).BitType.Number(8);
        var paths = source.SymmetricPathsClosureBackwards().ToArray();

        var destinations = paths.Select(p => p.Source).ToArray();

        destinations.Contains(source.Pointer).Assert();
        destinations.Contains(source.Align).Assert();
        destinations.Contains(source).Assert();
        (destinations.Length == 2).Assert();
    }

    [UnitTest]
    public void ClosureServiceOfAlignedNumberBackwards()
    {
        var source = new Root(null!).BitType.Number(8);
        var paths = source.SymmetricPathsClosureBackwards().ToArray();

        var destinations = paths.Select(p => p.Source).ToArray();

        destinations.Contains(source.Pointer).Assert();
        destinations.Contains(source.Align).Assert();
        destinations.Contains(source).Assert();
        (destinations.Length == 2).Assert();
    }

    [UnitTest]
    public void ClosureServiceOfNumber()
    {
        var source = new Root(null!).BitType.Number(1);
        var paths = Type.ConversionService.ClosureService.GetResult(source);
        var destinations = paths.Select(p => p.Destination).ToArray();

        destinations.Contains(source.Pointer).Assert();
        destinations.Contains(source.Align).Assert();
        destinations.Contains(source).Assert();
        (destinations.Length == 3).Assert();
    }

    [UnitTest]
    public void OfNumberPointer()
    {
        var number = new Root(null!).BitType.Number(4);
        var pointer = number.Pointer;
        var paths = pointer.SymmetricPathsClosure().ToArray();

        (paths.Select(p => p.Source).Distinct().Single() == pointer).Assert();

        var destinations = paths.Select(p => p.Destination).ToArray();

        destinations.Contains(number.Pointer).Assert();
        destinations.Contains(number.Align).Assert();
        destinations.Contains(number).Assert();
        (destinations.Length == 3).Assert();
    }
}