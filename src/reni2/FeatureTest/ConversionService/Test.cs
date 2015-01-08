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
        public void Closure()
        {
            var t = new Root(null).BitType.UniqueNumber(1);
            var tt = t.SymmetricFeatureClosure().Types().ToArray();
            Tracer.Assert(tt.Length == 3);
            Tracer.Assert(tt.Contains(t));
            Tracer.Assert(tt.Contains(t.UniqueAlign));
            Tracer.Assert(tt.Contains(t.UniquePointer));
        }
    }
}