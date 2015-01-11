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
            , ISimpleFeature
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
            int IContextReference.Order { get { return _order; } }
            Size IContextReference.Size { get { return Root.DefaultRefAlignParam.RefSize; } }
            [EnableDump]
            FunctionSyntax Syntax { get { return _parent._syntax; } }
        }

        [DisableDump]
        internal override CompoundView FindRecentCompoundView { get { return _compoundView; } }
        [DisableDump]
        internal override Root RootContext { get { return _compoundView.RootContext; } }
        [DisableDump]
        internal override bool Hllw { get { return true; } }
        [DisableDump]
        internal override string DumpPrintText { get { return _syntax.DumpPrintText; } }

        internal new Result DumpPrintTokenResult(Category category) => DumpPrintTypeNameResult(category);

        [DisableDump]
        IContextReference ObjectReference { get { return _objectReferenceCache.Value; } }

        //ISuffixFeature IFeaturePath<ISuffixFeature, DumpPrintToken>.GetFeature(DumpPrintToken target) { return Extension.Feature(DumpPrintTokenResult); }

        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }
        bool IFunctionFeature.IsImplicit { get { return _syntax.IsImplicit; } }
        TypeBase ISimpleFeature.TargetType { get { return this; } }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -3 && (category.HasCode);
            StartMethodDump(trace, category, argsType);
            try
            {
                BreakExecution();

                var functionType = Function(argsType);

                Dump("functionType", functionType);
                BreakExecution();

                var applyResult = functionType.ApplyResult(category);
                Tracer.Assert(category == applyResult.CompleteCategory);

                Dump("applyResult", applyResult);
                BreakExecution();

                var result = applyResult
                    .ReplaceAbsolute
                    (
                        _compoundView.Compound,
                        () => CodeBase.ReferenceCode(ObjectReference).ReferencePlus(_compoundView.CompoundSize),
                        () => CodeArgs.Create(ObjectReference)
                    );

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
        FunctionType Function(TypeBase argsType) { return _compoundView.Function(_syntax, argsType); }
        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta { get { return null; } }
        IMetaFunctionFeature IFeatureImplementation.Meta { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return this; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return this; } }
        Result ISimpleFeature.Result(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}