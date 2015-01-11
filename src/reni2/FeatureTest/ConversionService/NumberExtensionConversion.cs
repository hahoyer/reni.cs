using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Context;
using Reni.Type;
using Reni.Type.ConversionService;

namespace Reni.FeatureTest.ConversionService
{
    [TestFixture]
    public sealed class NumberExtensionConversion : DependantAttribute
    {
        [Test]
        public void Run()
        {
            var numberSmall = new Root(null).BitType.Number(4);
            var numberLarge = new Root(null).BitType.Number(8);
            var paths = ForcedConversions(new Path((TypeBase)numberSmall), new Path((TypeBase)numberLarge)).ToArray();

            Tracer.Assert(paths.Length == 1);
        }
    }
}