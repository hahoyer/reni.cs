using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class AccessFeature
        : DumpableObject
            , IFeatureImplementation
            , IValueFeature
    {
        static int _nextObjectId;

        internal AccessFeature(CompoundView compoundView, int position)
            : base(_nextObjectId++)
        {
            View = compoundView;
            Position = position;
            FunctionFeature = new ValueCache<IFunctionFeature>(ObtainFunctionFeature);
            StopByObjectIds();
        }

        [EnableDump]
        public CompoundView View { get; }
        [EnableDump]
        public int Position { get; }

        ValueCache<IFunctionFeature> FunctionFeature { get; }

        IContextMetaFunctionFeature IContextMetaFeatureImplementation.ContextMeta
            => (Statement as FunctionSyntax)?.ContextMetaFunctionFeature(View);

        IMetaFunctionFeature IMetaFeatureImplementation.Meta => (Statement as FunctionSyntax)?.MetaFunctionFeature(View);

        IFunctionFeature ITypedFeatureImplementation.Function => FunctionFeature.Value;

        IValueFeature ITypedFeatureImplementation.Value
        {
            get
            {
                var function = FunctionFeature.Value;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }

        Result IValueFeature.Result(Category category) => View.AccessViaObject(category, Position);

        TypeBase IValueFeature.TargetType => View.Type;

        CompileSyntax Statement => View
            .Compound
            .Syntax
            .Statements[Position];

        IFunctionFeature ObtainFunctionFeature()
        {

            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(View);

            var valueType = View.ValueType(Position);
            StopByObjectIds();
            return valueType.CheckedFeature?.Function;
        }
    }
}