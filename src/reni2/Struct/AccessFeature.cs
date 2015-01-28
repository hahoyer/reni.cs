using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.ReniSyntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class AccessFeature
        : DumpableObject
            , IFeatureImplementation
            , ISimpleFeature
    {
        static int _nextObjectId;

        internal AccessFeature(CompoundView compoundView, int position)
            : base(_nextObjectId++)
        {
            View = compoundView;
            Position = position;
            FunctionFeature = new ValueCache<IFunctionFeature>(ObtainFunctionFeature);
        }

        [EnableDump]
        public CompoundView View { get; }
        [EnableDump]
        public int Position { get; }

        ValueCache<IFunctionFeature> FunctionFeature { get; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta
            => (Statement as FunctionSyntax)?.ContextMetaFunctionFeature(View);

        IMetaFunctionFeature IFeatureImplementation.Meta => (Statement as FunctionSyntax)?.MetaFunctionFeature(View);

        IFunctionFeature IFeatureImplementation.Function => FunctionFeature.Value;

        ISimpleFeature IFeatureImplementation.Simple
        {
            get
            {
                var function = FunctionFeature.Value;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }

        Result ISimpleFeature.Result(Category category) => View
            .AccessViaContext(category, Position);

        TypeBase ISimpleFeature.TargetType => View.Type;

        CompileSyntax Statement => View
            .Compound
            .Syntax
            .Statements[Position];

        IFunctionFeature ObtainFunctionFeature()
        {
            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(View);

            return View.ValueType(Position).Feature?.Function;
        }
    }
}