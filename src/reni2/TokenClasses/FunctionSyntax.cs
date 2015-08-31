using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : SpecialSyntax
    {
        public static Checked<Syntax> Create
            (
            Checked<CompileSyntax> setter,
            bool isImplicit,
            bool isMetaFunction,
            Checked<CompileSyntax> getter)
            => new FunctionSyntax(setter?.Value, isImplicit, isMetaFunction, getter.Value)
                .Issues(setter?.Issues.plus(getter.Issues));

        internal CompileSyntax Getter { get; }

        internal CompileSyntax Setter { get; }

        bool IsMetaFunction { get; }
        internal bool IsImplicit { get; }

        public FunctionSyntax
            (
            CompileSyntax setter,
            bool isImplicit,
            bool isMetaFunction,
            CompileSyntax getter)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return Setter;
                yield return Getter;
            }
        }

        string Tag
            => (IsMetaFunction ? "{0}{0}" : "{0}")
                .ReplaceArgs("/{0}\\")
                .ReplaceArgs(IsImplicit ? "!" : "");

        internal override Result ResultForCache(ContextBase context, Category category)
            => context
                .FindRecentCompoundView
                .FunctionalType(this)
                .Result(category);

        protected override bool GetIsLambda() => true;

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