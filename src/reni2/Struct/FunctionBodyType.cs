using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
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
        sealed class ContextReference : DumpableObject, IContextReference
        {
            readonly int Order;

            [Node]
            readonly FunctionBodyType Parent;

            public ContextReference(FunctionBodyType parent)
                : base(parent.ObjectId)
            {
                Order = Closures.NextOrder++;
                Parent = parent;
                StopByObjectIds(-5);
            }

            [EnableDump]
            FunctionSyntax Syntax => Parent.Syntax;

            int IContextReference.Order => Order;
        }

        [EnableDump]
        [Node]
        internal readonly FunctionSyntax Syntax;

        public FunctionBodyType(CompoundView compoundView, FunctionSyntax syntax)
        {
            FindRecentCompoundView = compoundView;
            Syntax = syntax;
        }

        [DisableDump]
        [field: EnableDump]
        [field: Node]
        internal override CompoundView FindRecentCompoundView { get; }

        [DisableDump]
        internal override Root Root => FindRecentCompoundView.Root;

        [DisableDump]
        internal override bool IsHollow => true;

        [DisableDump]
        internal override IImplementation FunctionDeclarationForType => this;

        [DisableDump]
        internal IEnumerable<FunctionType> Functions => FindRecentCompoundView.Functions(Syntax);

        CompoundView IChild<CompoundView>.Parent => FindRecentCompoundView;

        Result IConversion.Execute(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        TypeBase IConversion.Source => this;
        IFunction IEvalImplementation.Function => this;
        IValue IEvalImplementation.Value => this;

        bool IFunction.IsImplicit => Syntax.IsImplicit;

        Result IFunction.Result(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -49 && category.Replenished.HasClosures;
            StartMethodDump(trace, category, argsType);
            try
            {
                BreakExecution();

                var functionType = Function(argsType.AssertNotNull());

                Dump("functionType", functionType);
                BreakExecution();

                var result = functionType.ApplyResult(category);
                (category == result.CompleteCategory).Assert();

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        IMeta IMetaImplementation.Function => null;

        IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        Result IValue.Execute(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        protected override CodeBase DumpPrintCode()
            => CodeBase.DumpPrintText(Syntax.Tag);

        new Result DumpPrintTokenResult(Category category)
            => VoidType
                .Result(category, DumpPrintCode);

        FunctionType Function(TypeBase argsType) => FindRecentCompoundView.Function(Syntax, argsType.AssertNotNull());
    }
}