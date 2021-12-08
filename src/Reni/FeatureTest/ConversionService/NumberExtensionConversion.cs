using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.Context;
using Reni.Type;

namespace Reni.FeatureTest.ConversionService
{
    [UnitTest]
    public sealed class NumberExtensionConversion : DependenceProvider
    {
        [UnitTest]
        public void Run4To8Forced()
        {
            var numberSmall = new Root(null).BitType.Number(4);
            var numberLarge = new Root(null).BitType.Number(8);
            var paths = Reni.Type.ConversionService.ForcedConversions(new ConversionPath((TypeBase)numberSmall), new ConversionPath((TypeBase)numberLarge)).ToArray();

            (paths.Length == 1).Assert();
        }
        [UnitTest]
        public void Run1To3Forced()
        {
            var numberSmall = new Root(null).BitType.Number(1);
            var numberLarge = new Root(null).BitType.Number(3);
            var paths = Reni.Type.ConversionService.ForcedConversions(new ConversionPath((TypeBase)numberSmall), new ConversionPath((TypeBase)numberLarge)).ToArray();

            (paths.Length == 1).Assert();
        }

        [UnitTest]
        [Closure]
        public void Run1To3()
        {
            var source = new Root(null).BitType.Number(1);
            var destination = new Root(null).BitType.Number(3);
            var paths = Reni.Type.ConversionService.FindPath(source, destination);

            (paths!= null).Assert();
        }

    }
}