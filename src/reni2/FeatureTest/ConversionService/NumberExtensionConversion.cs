using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Context;
using Reni.Type;

namespace Reni.FeatureTest.ConversionService
{
    [UnitTest]
    public sealed class NumberExtensionConversion : DependantAttribute
    {
        [UnitTest]
        public void Run4to8Forced()
        {
            var numberSmall = new Root(null).BitType.Number(4);
            var numberLarge = new Root(null).BitType.Number(8);
            var paths = Reni.Type.ConversionService.ForcedConversions(new ConversionPath((TypeBase)numberSmall), new ConversionPath((TypeBase)numberLarge)).ToArray();

            Tracer.Assert(paths.Length == 1);
        }
        [UnitTest]
        public void Run1to3Forced()
        {
            var numberSmall = new Root(null).BitType.Number(1);
            var numberLarge = new Root(null).BitType.Number(3);
            var paths = Reni.Type.ConversionService.ForcedConversions(new ConversionPath((TypeBase)numberSmall), new ConversionPath((TypeBase)numberLarge)).ToArray();

            Tracer.Assert(paths.Length == 1);
        }

        [UnitTest]
        [Closure]
        public void Run1to3()
        {
            var source = new Root(null).BitType.Number(1);
            var destination = new Root(null).BitType.Number(3);
            var paths = Reni.Type.ConversionService.FindPath(source, destination);

            Tracer.Assert(paths!= null);
        }

    }
}