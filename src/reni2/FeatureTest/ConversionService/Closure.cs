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
    public sealed class Closure : DependantAttribute
    {
        [Test]
        public void OfNumber()
        {
            var number = new Root(null).BitType.UniqueNumber(1);
            var types = number.SymmetricFeatureClosure().Types().ToArray();

            Tracer.Assert(types.Length == 3);
            Tracer.Assert(types.Contains(number));
            Tracer.Assert(types.Contains(number.UniqueAlign));
            Tracer.Assert(types.Contains(number.UniquePointer));
        }
        [Test]
        public void OfNumberPointer()
        {
            var number = new Root(null).BitType.UniqueNumber(4);
            var pointer = number.UniquePointer;
            var paths = pointer.SymmetricPathsClosure().ToArray();
            var destinations = paths.Select(p => p.Destination).ToArray();

            Tracer.Assert(paths.Length == 3);
            Tracer.Assert(paths.Select(p => p.Source).Distinct().Single() == pointer);
            Tracer.Assert(destinations.Length == 3);
            Tracer.Assert(destinations.Contains(number.UniquePointer));
            Tracer.Assert(destinations.Contains(number.UniqueAlign));
            Tracer.Assert(destinations.Contains(number));
        }
    }

    [TestFixture]
    public sealed class NumberExtensionConversion : DependantAttribute
    {
        [Test]
        public void Run()
        {
            var numberSmall = new Root(null).BitType.UniqueNumber(4);
            var numberLarge = new Root(null).BitType.UniqueNumber(8);
            var paths = ForcedConversions(new Path((TypeBase)numberSmall), new Path((TypeBase)numberLarge)).ToArray();

            Tracer.Assert(paths.Length == 1);
        }
    }
}