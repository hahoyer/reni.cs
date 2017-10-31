using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
            , IFunction
            , IValue
            , IConversion
            , IImplementation
            , IChild<CompoundView>
        , ISymbolProvider<DumpPrintToken>
    {
        [EnableDump]
        [Node]
        readonly CompoundView CompoundView;
        [EnableDump]
        [Node]
        internal readonly FunctionSyntax Syntax;

        public FunctionBodyType(CompoundView compoundView, FunctionSyntax syntax)
        {
            CompoundView = compoundView;
            Syntax = syntax;
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
            [EnableDump]
            FunctionSyntax Syntax => _parent.Syntax;
        }

        [DisableDump]
        internal override CompoundView FindRecentCompoundView => CompoundView;
        [DisableDump]
        internal override Root Root => CompoundView.Root;
        [DisableDump]
        internal override bool Hllw => true;

        [DisableDump]
        internal override IImplementation FunctionDeclarationForType => this;

        bool IFunction.IsImplicit => Syntax.IsImplicit;

        Result IFunction.Result(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -49 && category.Replenished.HasExts;
            StartMethodDump(trace, category, argsType);
            try
            {
                BreakExecution();

                var functionType = Function(argsType.AssertNotNull());

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

        IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        protected override CodeBase DumpPrintCode()
            => CodeBase.DumpPrintText(Syntax.Tag);

        new Result DumpPrintTokenResult(Category category)
            => VoidType
                .Result(category, DumpPrintCode);

        [DisableDump]
        internal IEnumerable<FunctionType> Functions => CompoundView.Functions(Syntax);

        FunctionType Function(TypeBase argsType) => CompoundView.Function(Syntax, argsType.AssertNotNull());

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

        CompoundView IChild<CompoundView>.Parent => CompoundView;
    }
}