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
        private readonly AccessType _type;

        internal AssignmentFeature(AccessType type) { _type = type; }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return _type.ApplyAssignment(category, argsType); }
        string IDumpShortProvider.DumpShort() { return _type.DumpShort() + " :="; }
    }
}