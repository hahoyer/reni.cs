using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Context;

namespace Reni.Type
{
    internal sealed class AssignmentFeature : ReniObject, IFunctionalFeature
    {
        [EnableDump]
        private readonly Reference _referenceType;

        internal AssignmentFeature(Reference referenceType) { _referenceType = referenceType; }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return _referenceType.ApplyAssignment(category, argsType); }
        string IDumpShortProvider.DumpShort() { return _referenceType.DumpShort() + " :="; }
    }
}