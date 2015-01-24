using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Context;
using Reni.Type;

namespace Reni.FeatureTest.ConversionService
{
    [TestFixture]
    public sealed class NumberExtensionConversion : DependantAttribute
    {
        [Test]
        public void Run4to8Forced()
        {
            var numberSmall = new Root(null).BitType.Number(4);
            var numberLarge = new Root(null).BitType.Number(8);
            var paths = Reni.Type.ConversionService.ForcedConversions(new Reni.Type.ConversionService.Path((TypeBase)numberSmall), new Reni.Type.ConversionService.Path((TypeBase)numberLarge)).ToArray();

            Tracer.Assert(paths.Length == 1);
        }
        [Test]
        public void Run1to3Forced()
        {
            var numberSmall = new Root(null).BitType.Number(1);
            var numberLarge = new Root(null).BitType.Number(3);
            var paths = Reni.Type.ConversionService.ForcedConversions(new Reni.Type.ConversionService.Path((TypeBase)numberSmall), new Reni.Type.ConversionService.Path((TypeBase)numberLarge)).ToArray();

            Tracer.Assert(paths.Length == 1);
        }

        [Test]
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