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

        [EnableDump]
        readonly CompoundView _compoundView;
        [EnableDump]
        readonly int _position;
        readonly ValueCache<IFunctionFeature> _functionFeature;

        internal AccessFeature(CompoundView compoundView, int position)
            : base(_nextObjectId++)
        {
            _compoundView = compoundView;
            _position = position;
            _functionFeature = new ValueCache<IFunctionFeature>(ObtainFunctionFeature);
        }

        Result ISimpleFeature.Result(Category category)
        {
            return _compoundView
                .AccessViaThisReference(category, _position);
        }

        TypeBase ISimpleFeature.TargetType { get { return _compoundView.Type; } }

        CompileSyntax Statement
        {
            get
            {
                return _compoundView
                    .Compound
                    .Syntax
                    .Statements[_position];
            }
        }

        protected override IContextMetaFunctionFeature ContextMeta
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.ContextMetaFunctionFeature(_compoundView);
            }
        }
        protected override IMetaFunctionFeature Meta
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.MetaFunctionFeature(_compoundView);
            }
        }

        protected override IFunctionFeature Function { get { return _functionFeature.Value; } }

        IFunctionFeature ObtainFunctionFeature()
        {
            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(_compoundView);

            var feature = _compoundView.ValueType(_position).Feature;
            if(feature == null)
                return null;
            return feature.Function;
        }

        protected override ISimpleFeature Simple
        {
            get
            {
                var function = _functionFeature.Value;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }
    }
}