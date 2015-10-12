using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
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
            , IFunction
            , IValue
            , IConversion
            , IImplementation
    {
        [EnableDump]
        [Node]
        readonly CompoundView _compoundView;
        [EnableDump]
        [Node]
        readonly FunctionSyntax _syntax;

        public FunctionBodyType(CompoundView compoundView, FunctionSyntax syntax)
        {
            _compoundView = compoundView;
            _syntax = syntax;
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
                StopByObjectIds(-5);
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
        internal override IImplementation FuncionDeclarationForType => this;

        bool IFunction.IsImplicit => _syntax.IsImplicit;

        Result IFunction.Result(Category category, TypeBase argsType)
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

        IMeta IMetaImplementation.Function => null;
        IFunction IEvalImplementation.Function => this;
        IValue IEvalImplementation.Value => this;

        TypeBase IConversion.Source => this;

        Result IConversion.Execute(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        Result IValue.Execute(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        ResultCache.IResultProvider IFunction.FindSource(IContextReference ext)
            => _syntax.FindSource(ext, _compoundView.Context);

        ResultCache.IResultProvider IValue.FindSource(IContextReference ext)
        {
            NotImplementedMethod(ext);
            return null;
        }
    }
}