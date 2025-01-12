using System.Diagnostics;
using hw.UnitTest;

namespace ReniTest;

[UnitTest]
class DebuggerDisplayTest
{
    [DebuggerDisplay("click here ")]
    public sealed class Sample
    {
        [UsedImplicitly]
        int[] Data = { 1, 3, 4 };

        IEnumerable<int> Data2 => Data.Where(x => true);

        public Sample() => "????".Log();
    }

    [UnitTest]
    public void TestMethod()
    {
        var xxx = new ValueCache<Sample>(() => new());

        var yyy = xxx.Value;
    }
}