using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class AssignmentFeature : ReniObject, IFunctionalFeature
    {
        [IsDumpEnabled(true)]
        private readonly Field _type;

        public AssignmentFeature(Field type) { _type = type; }

        Result IFunctionalFeature.ContextOperatorFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return _type.ApplyAssignment(category, argsType); }
        string IDumpShortProvider.DumpShort() { return _type.DumpShort() + " :="; }
    }
}