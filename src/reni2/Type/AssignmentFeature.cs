using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    internal sealed class AssignmentFeature : ReniObject, IFunctionalFeature
    {
        [EnableDump]
        private readonly AutomaticReferenceType _automaticReferenceTypeType;

        internal AssignmentFeature(AutomaticReferenceType automaticReferenceTypeType) { _automaticReferenceTypeType = automaticReferenceTypeType; }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return _automaticReferenceTypeType.ApplyAssignment(category, argsType); }
        string IDumpShortProvider.DumpShort() { return _automaticReferenceTypeType.DumpShort() + " :="; }
    }
}