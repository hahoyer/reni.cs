using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"i: (400000) enable_reassign; f: /\i > 0 then (i := (i - 1)enable_cut; f());f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionSmall]
    //[LowPriority]
    public sealed class PrimitiveRecursiveFunctionHuge : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}