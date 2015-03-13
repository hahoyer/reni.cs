using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [TargetSet("5, (^^ _A_T_ 0 + ^^ _A_T_ 0)dump_print", "10")]
    public sealed class AccessAndAdd : CompilerTest
    {}
}