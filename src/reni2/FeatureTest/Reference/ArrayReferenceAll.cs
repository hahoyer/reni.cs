using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [ArrayReferenceCopyAssign]
    [ArrayReferenceCopy]
    [ArrayReferenceDumpLoop]
    [ArrayReferenceByInstance]
    public sealed class ArrayReferenceAll : CompilerTest {}
}