using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.Validation;

namespace Reni.FeatureTest
{
    [UnitTest]
    [SimpleAssignment]
    [UndefinedContextSymbol]
    [UndefinedSymbol]
    [UseOfUndefinedContextSymbol]
    [IndirectUseOfUndefinedContextSymbol]
    [AllScopeHandling]
    [Output("Ha")]
    public sealed class ComplexContext : CompilerTest
    {
        protected override string Target => "FeatureTest\\ComplexContext.reni".ToSmbFile().String;
    }
}