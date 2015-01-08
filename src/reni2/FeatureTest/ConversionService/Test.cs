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
    public sealed class Test : DependantAttribute
    {
        [hw.UnitTest.Test]
        public void ClosureOfNumber()
        {
            var number = new Root(null).BitType.UniqueNumber(1);
            var types = number.SymmetricFeatureClosure().Types().ToArray();

            Tracer.Assert(types.Length == 3);
            Tracer.Assert(types.Contains(number));
            Tracer.Assert(types.Contains(number.UniqueAlign));
            Tracer.Assert(types.Contains(number.UniquePointer));
        }
        [hw.UnitTest.Test]
        public void ClosureNumberPointer()
        {
            var number = new Root(null).BitType.UniqueNumber(4);
            var pointer = number.UniquePointer;
            var paths = pointer.SymmetricPathsClosure().ToArray();
            var destinations = paths.Select(p => p.Destination).ToArray();

            Tracer.Assert(paths.Length == 3);
            Tracer.Assert(paths.Select(p=>p.Source).Distinct().Single() == pointer);
            Tracer.Assert(destinations.Length == 3);
            Tracer.Assert(destinations.Contains(number.UniquePointer));
            Tracer.Assert(destinations.Contains(number.UniqueAlign));
            Tracer.Assert(destinations.Contains(number));
        }
    }
}