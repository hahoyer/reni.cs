using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
    [ArrayReferenceCopyAssign]
    [ArrayReferenceCopy]
    [ArrayReferenceDumpLoop]
    [ArrayReferenceByInstance]
    public sealed class ArrayReferenceAll : CompilerTest {}
}