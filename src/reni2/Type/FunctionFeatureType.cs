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
        internal override bool Hllw { get { return false; } }
        [DisableDump]
        bool IFunctionFeature.IsImplicit { get { return false; } }
        [DisableDump]
        internal override bool IsLambda { get { return true; } }
        [DisableDump]
        IContextReference IFunctionFeature.ObjectReference { get { return _functionalFeature.ObjectReference; } }
        [DisableDump]
        internal override Root RootContext { get { return _functionalFeature.RootContext; } }
        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }
        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + _functionalFeature.NodeDump + ")"; }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            return _functionalFeature.ApplyResult(category, argsType);
        }
    }
}