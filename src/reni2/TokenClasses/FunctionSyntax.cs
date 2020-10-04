using System.Collections.Generic;
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

        FunctionSyntax
        (
            ValueSyntax setter,
            bool isImplicit,
            bool isMetaFunction,
            ValueSyntax getter, BinaryTree binaryTree
        )
            : base(binaryTree)
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

        internal static Result<ValueSyntax> Create
        (
            BinaryTree left, bool isImplicit, bool isMetaFunction, BinaryTree right, BinaryTree binaryTree
            , ISyntaxScope scope
        )
        {
            var leftValue = left?.Syntax(scope);
            var rightValue = right?.Syntax(scope);
            var target = new FunctionSyntax
                (leftValue?.Target, isImplicit, isMetaFunction, rightValue?.Target, binaryTree);
            var issues = leftValue?.Issues.plus(rightValue?.Issues);
            return new Result<ValueSyntax>(target, issues);
        }

        protected override IEnumerable<ValueSyntax> GetChildren() => T(Getter, Setter);

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