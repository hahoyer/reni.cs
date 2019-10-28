using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
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
            (Syntax left, bool isImplicit, bool isMetaFunction, Syntax right, Syntax syntax)
        {
            var leftvalue = left?.Value;
            var rightvalue = right?.Value;
            var target = new FunctionSyntax
                (leftvalue?.Target, isImplicit, isMetaFunction, rightvalue?.Target,syntax);
            var issues = leftvalue?.Issues.plus(rightvalue?.Issues);
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
            Parser.Value getter, Syntax syntax)
            : base(syntax)
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