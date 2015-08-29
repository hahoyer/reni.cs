using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class FunctionFeatureType : TypeBase, IFunctionFeature
    {
        readonly FunctionalFeature _functionalFeature;

        internal FunctionFeatureType(FunctionalFeature functionalFeature) { _functionalFeature = functionalFeature; }

        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        bool IFunctionFeature.IsImplicit => false;
        [DisableDump]
        internal override Root RootContext => _functionalFeature.RootContext;
        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override string GetNodeDump() => base.GetNodeDump() + "(" + _functionalFeature.NodeDump + ")";

        Result IFunctionFeature.Result(Category category, TypeBase argsType)
            => _functionalFeature.ApplyResult(category, argsType);
    }
}