using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class AccessFeature : DumpableObject, ISimpleFeature, IFeatureImplementation
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

        IContextMetaFunctionFeature IFeatureImplementation.ContextMetaFunction
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.ContextMetaFunctionFeature(_structure);
                
            }
        }
        IMetaFunctionFeature IFeatureImplementation.MetaFunction
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.MetaFunctionFeature(_structure);
            }
        }

        IFunctionFeature IFeatureImplementation.Function { get { return FunctionFeature; } }
        IFunctionFeature FunctionFeature { get { return _functionFeature.Value; } }

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

        ISimpleFeature IFeatureImplementation.Simple
        {
            get
            {
                var function = FunctionFeature;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }
    }
}