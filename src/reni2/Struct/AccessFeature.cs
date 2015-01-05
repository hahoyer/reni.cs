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
    sealed class AccessFeature : EmptyFeatureImplementation, ISimpleFeature
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

        Result ISimpleFeature.Result(Category category) => View
            .AccessViaThisReference(category, Position);

        TypeBase ISimpleFeature.TargetType => View.Type;

        CompileSyntax Statement => View
            .Compound
            .Syntax
            .Statements[Position];

        protected override IContextMetaFunctionFeature ContextMeta
            => (Statement as FunctionSyntax)?.ContextMetaFunctionFeature(View);

        protected override IMetaFunctionFeature Meta => (Statement as FunctionSyntax)?.MetaFunctionFeature(View);

        protected override IFunctionFeature Function => FunctionFeature.Value;

        IFunctionFeature ObtainFunctionFeature()
        {
            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(View);

            return View.ValueType(Position).Feature?.Function;
        }

        protected override ISimpleFeature Simple
        {
            get
            {
                var function = FunctionFeature.Value;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }
    }
}