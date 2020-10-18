using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class FunctionSyntax : ValueSyntax
    {
        [EnableDumpExcept(null)]
        internal ValueSyntax Getter { get; }

        [EnableDump]
        [EnableDumpExcept(false)]
        internal bool IsImplicit { get; }

        [EnableDumpExcept(null)]
        internal ValueSyntax Setter { get; }

        bool IsMetaFunction { get; }

        internal FunctionSyntax
        (
            ValueSyntax setter,
            bool isImplicit,
            bool isMetaFunction,
            ValueSyntax getter, BinaryTree anchor
        )
            : base(anchor)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        [DisableDump]
        internal string Tag
            => "@"
               + (IsMetaFunction? "@" : "")
               + (IsImplicit? "!" : "");

        [DisableDump]
        internal override bool IsLambda => true;

        [DisableDump]
        internal override int LeftDirectChildCount => 1;

        [DisableDump]
        protected override int DirectChildCount => 2;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Setter
                , 1 => Getter
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