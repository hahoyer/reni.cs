using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;
using Reni.Basics;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class AccessFeature : EmptyFeatureImplementation, ISimpleFeature
    {
        static int _nextObjectId;

        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly int _position;
        readonly ValueCache<IFunctionFeature> _functionFeature;

        internal AccessFeature(Structure structure, int position)
            : base(_nextObjectId++)
        {
            _structure = structure;
            _position = position;
            _functionFeature = new ValueCache<IFunctionFeature>(ObtainFunctionFeature);
        }

        Result ISimpleFeature.Result(Category category)
        {
            return _structure
                .AccessViaThisReference(category, _position);
        }

        CompileSyntax Statement
        {
            get
            {
                return _structure
                    .ContainerContextObject
                    .Container
                    .Statements[_position];
            }
        }

        protected override IContextMetaFunctionFeature ContextMeta
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.ContextMetaFunctionFeature(_structure);
                
            }
        }
        protected override IMetaFunctionFeature Meta
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.MetaFunctionFeature(_structure);
            }
        }

        protected override IFunctionFeature Function { get { return _functionFeature.Value; } }

        IFunctionFeature ObtainFunctionFeature()
        {
            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(_structure);

            var feature = _structure.ValueType(_position).Feature;
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