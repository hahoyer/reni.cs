using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;

namespace Reni.Type
{
    internal sealed class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature
    {
        [DumpData(true)]
        private readonly Struct.Reference _type;

        public AssignmentFeature(Struct.Reference type) { _type = type; }

        Result IFeature.Apply(Category category)
        {
            return _type
                .CreateFunctionalType(this)
                .CreateArgResult(category);
        }

        TypeBase IFeature.DefiningType() { return _type; }
        Result IFunctionalFeature.ContextOperatorFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            return _type
                .ApplyAssignment(category, functionalResult, argsResult);
        }
        string IDumpShortProvider.DumpShort() { return _type.DumpShort() + " :="; }
    }
}