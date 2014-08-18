using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"f: /\ .;f(2) dump_print;")]
    [Output("2")]
    [InnerAccess]
    [SomeVariables]
    public sealed class Function : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}