using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    [Obsolete("",true)]
    internal sealed class DumpPrintFeature : ReniObject, IFeature
    {
        private readonly Struct.Reference _reference;

        public DumpPrintFeature(Struct.Reference reference) { _reference = reference; }

        Result IFeature.Apply(Category category) { return _reference.DumpPrint(category); }

        TypeBase IFeature.DefiningType() { return _reference; }
    }


    internal sealed class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature, ISearchPath<IFeature, Reference>
    {
        [DumpData(true)]
        private readonly Struct.Type _type;

        public AssignmentFeature(Struct.Type type) { _type = type; }

        Result IFeature.Apply(Category category) { return _type.CreateFunctionalType(this).CreateArgResult(category); }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult) { return _type.ApplyAssignment(category, functionalResult, argsResult); }

        TypeBase IFeature.DefiningType()
        {
            return _type;
        }

        string IDumpShortProvider.DumpShort() { return _type.DumpShort() + " :="; }
        IFeature ISearchPath<IFeature, Reference>.Convert(Reference type)
        {
            Tracer.Assert(type.RefAlignParam == _type.RefAlignParam);
            return this;
        }
    }
}