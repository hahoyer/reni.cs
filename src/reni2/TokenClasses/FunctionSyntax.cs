using System.Collections.Generic;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : Parser.Value
    {
        internal static Result<Parser.Value> Create
            (BinaryTree left, bool isImplicit, bool isMetaFunction, BinaryTree right, BinaryTree binaryTree, IValuesScope scope)
        {
            var leftValue = left?.Value(scope);
            var rightValue = right?.Value(scope);
            var target = new FunctionSyntax
                (leftValue?.Target, isImplicit, isMetaFunction, rightValue?.Target,binaryTree);
            var issues = leftValue?.Issues.plus(rightValue?.Issues);
            return new Result<Parser.Value>(target, issues);
        }

        internal Parser.Value Getter { get; }
        internal Parser.Value Setter { get; }

        bool IsMetaFunction { get; }
        internal bool IsImplicit { get; }

        FunctionSyntax
            (
            Parser.Value setter,
            bool isImplicit,
            bool isMetaFunction,
            Parser.Value getter, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        internal string Tag
            => (IsMetaFunction ? "{0}{0}" : "{0}")
                .ReplaceArgs("/{0}\\")
                .ReplaceArgs(IsImplicit ? "!" : "");

        protected override IEnumerable<Parser.Value> GetChildren() => T(Getter,Setter);

        internal override Result ResultForCache(ContextBase context, Category category)
            => context
                .FindRecentCompoundView
                .FunctionalType(this)
                .Result(category);

        internal override bool IsLambda => true;

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