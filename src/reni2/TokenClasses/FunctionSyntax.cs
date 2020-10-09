using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : ValueSyntax
    {
        internal ValueSyntax Getter { get; }
        internal bool IsImplicit { get; }
        internal ValueSyntax Setter { get; }

        bool IsMetaFunction { get; }

        internal FunctionSyntax
        (
            ValueSyntax setter,
            bool isImplicit,
            bool isMetaFunction,
            ValueSyntax getter, BinaryTree target
        )
            : base(target)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        internal string Tag
            => (IsMetaFunction? "{0}{0}" : "{0}")
                .ReplaceArgs("/{0}\\")
                .ReplaceArgs(IsImplicit? "!" : "");

        internal override bool IsLambda => true;

        protected override int DirectNodeCount => 3;

        protected override Syntax GetDirectNode(int index)
            => index switch
            {
                0 => Getter
                , 1 => this
                , 2 => Setter
                , _ => null
            };

        internal override Result ResultForCache(ContextBase context, Category category)
            => context
                .FindRecentCompoundView
                .FunctionalType(this)
                .Result(category);

        internal IMeta MetaFunctionFeature(CompoundView compoundView)
        {
            if(!IsMetaFunction)
                return null;

            NotImplementedMethod(compoundView);
            return null;
        }

        internal IFunction FunctionFeature(CompoundView compoundView)
        {
            if(IsMetaFunction)
                return null;

            return new FunctionBodyType(compoundView, this);
        }

        protected override string GetNodeDump() => Setter?.NodeDump ?? "" + Tag + Getter.NodeDump;
    }
}