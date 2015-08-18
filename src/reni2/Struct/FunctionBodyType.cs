using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionBodyType
        : TypeBase
            , IFunctionFeature
            , IValueFeature
            , IFeatureImplementation
    {
        [EnableDump]
        [Node]
        readonly CompoundView _compoundView;
        [EnableDump]
        [Node]
        readonly FunctionSyntax _syntax;
        readonly ValueCache<IContextReference> _objectReferenceCache;

        public FunctionBodyType(CompoundView compoundView, FunctionSyntax syntax)
        {
            _compoundView = compoundView;
            _syntax = syntax;
            _objectReferenceCache = new ValueCache<IContextReference>(() => new ContextReference(this));
        }

        sealed class ContextReference : DumpableObject, IContextReference
        {
            [Node]
            readonly FunctionBodyType _parent;
            readonly int _order;
            public ContextReference(FunctionBodyType parent)
                : base(parent.ObjectId)
            {
                _order = CodeArgs.NextOrder++;
                _parent = parent;
                StopByObjectId(-5);
            }
            int IContextReference.Order => _order;
            Size IContextReference.Size => Root.DefaultRefAlignParam.RefSize;
            [EnableDump]
            FunctionSyntax Syntax => _parent._syntax;
        }

        [DisableDump]
        internal override CompoundView FindRecentCompoundView => _compoundView;
        [DisableDump]
        internal override Root RootContext => _compoundView.RootContext;
        [DisableDump]
        internal override bool Hllw => true;

        [DisableDump]
        IContextReference ObjectReference => _objectReferenceCache.Value;

        IContextReference IFunctionFeature.ObjectReference => ObjectReference;
        bool IFunctionFeature.IsImplicit => _syntax.IsImplicit;
        TypeBase IValueFeature.TargetType => this;

        Result IFunctionFeature.Result(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -30 && (category.HasCode);
            StartMethodDump(trace, category, argsType);
            try
            {
                BreakExecution();

                var functionType = Function(argsType);

                Dump("functionType", functionType);
                BreakExecution();

                var result = functionType.ApplyResult(category);
                Tracer.Assert(category == result.CompleteCategory);

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        FunctionType Function(TypeBase argsType) => _compoundView.Function(_syntax, argsType);

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => this;
        IValueFeature IFeatureImplementation.Value => this;

        Result IValueFeature.Result(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}