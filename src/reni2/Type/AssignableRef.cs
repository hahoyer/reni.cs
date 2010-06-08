using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature
    {
        [DumpData(true)]
        private readonly Struct.Reference _type;

        public AssignmentFeature(Struct.Reference type) { _type = type; }

        Result IFeature.Apply(Category category) { return _type.CreateFunctionalType(this).CreateArgResult(category); }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            return _type
                .ApplyAssignment(category, functionalResult, argsResult);
        }

        TypeBase IFeature.DefiningType() { return _type; }

        string IDumpShortProvider.DumpShort() { return _type.DumpShort() + " :="; }
    }
}