using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature
{
    sealed class CallDescriptor : FeatureDescriptor
    {
        public CallDescriptor(IFeatureImplementation feature, ConversionService.Path converterPath)
        {
            ConverterPath = converterPath;
            Feature = feature;
        }

        ConversionService.Path ConverterPath { get; }
        [DisableDump]
        protected override TypeBase Type => ConverterPath.Destination;
        protected override IContextReference ObjectReference => ConverterPath.Destination.CheckedReference.AssertNotNull();
        [DisableDump]
        protected override IFeatureImplementation Feature { get; }
    }

    sealed class FunctionalObjectDescriptor : FeatureDescriptor
    {
        readonly ContextBase _context;
        readonly CompileSyntax _left;
        internal FunctionalObjectDescriptor(ContextBase context, CompileSyntax left)
        {
            _context = context;
            _left = left;
        }
        protected override TypeBase Type => _context.Type(_left);
        protected override IContextReference ObjectReference
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        protected override IFeatureImplementation Feature => Type.CheckedFeature;
    }

    sealed class FunctionalArgDescriptor : FeatureDescriptor
    {
        readonly ContextBase _context;
        internal FunctionalArgDescriptor(ContextBase context) { _context = context; }

        [DisableDump]
        FunctionBodyType FunctionBodyType => (FunctionBodyType) _context.ArgReferenceResult(Category.Type).Type;
        [DisableDump]
        CompoundView CompoundView => FunctionBodyType.FindRecentCompoundView;
        [DisableDump]
        protected override TypeBase Type => CompoundView.Type;
        protected override IContextReference ObjectReference
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        [DisableDump]
        protected override IFeatureImplementation Feature => FunctionBodyType.CheckedFeature;
        public Result Result(Category category, ContextBase context, CompileSyntax right)
            => base.Result(category, context, right, CompoundView.ObjectPointerViaContext);
    }

    sealed class ContextCallDescriptor : FeatureDescriptor
    {
        [EnableDump]
        readonly ContextBase _definingItem;
        [EnableDump]
        readonly IFeatureImplementation _feature;

        public ContextCallDescriptor(ContextBase definingItem, IFeatureImplementation feature)
        {
            _definingItem = definingItem;
            _feature = feature;
        }

        [DisableDump]
        protected override TypeBase Type => _definingItem.FindRecentCompoundView.Type;
        protected override IContextReference ObjectReference => _definingItem.FindRecentCompoundView.Compound;

        [DisableDump]
        protected override IFeatureImplementation Feature => _feature;
    }
}