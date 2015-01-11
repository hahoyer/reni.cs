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
    public sealed class Closure : DependantAttribute
    {
        [Test]
        public void OfNumber()
        {
            var number = new Root(null).BitType.Number(1);
            var types = number.SymmetricFeatureClosure().Types().ToArray();

            Tracer.Assert(types.Contains(number));
            Tracer.Assert(types.Contains(number.Align));
            Tracer.Assert(types.Contains(number.Pointer));
            Tracer.Assert(types.Length == 3);
        }

        [Test]
        public void ClosureServiceOfNumberBackwards()
        {
            var source= new Root(null).BitType.Number(8);
            var paths = source.SymmetricPathsClosureBackwards().ToArray();

            var destinations = paths.Select(p => p.Source).ToArray();

            Tracer.Assert(destinations.Contains(source.Pointer));
            Tracer.Assert(destinations.Contains(source.Align));
            Tracer.Assert(destinations.Contains(source));
            Tracer.Assert(destinations.Length == 3);
        }

        [Test]
        public void ClosureServiceOfNumber()
        {
            var source = new Root(null).BitType.Number(1);
            var paths = Type.ConversionService.ClosureService.Result(source);
            var destinations = paths.Select(p => p.Destination).ToArray();

            Tracer.Assert(destinations.Contains(source.Pointer));
            Tracer.Assert(destinations.Contains(source.Align));
            Tracer.Assert(destinations.Contains(source));
            Tracer.Assert(destinations.Length == 3);
        }

        [Test]
        public void OfNumberPointer()
        {
            var number = new Root(null).BitType.Number(4);
            var pointer = number.Pointer;
            var paths = pointer.SymmetricPathsClosure().ToArray();

            Tracer.Assert(paths.Select(p => p.Source).Distinct().Single() == pointer);

            var destinations = paths.Select(p => p.Destination).ToArray();

            Tracer.Assert(destinations.Contains(number.Pointer));
            Tracer.Assert(destinations.Contains(number.Align));
            Tracer.Assert(destinations.Contains(number));
            Tracer.Assert(destinations.Length == 3);
        }

    }
}