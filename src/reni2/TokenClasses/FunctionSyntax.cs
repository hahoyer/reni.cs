using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : SpecialSyntax
    {
        internal CompileSyntax Getter { get; }
        bool IsMetaFunction { get; }
        internal CompileSyntax Setter { get; }
        internal bool IsImplicit { get; }

        public FunctionSyntax
            (
            Token token,
            CompileSyntax setter,
            bool isImplicit,
            bool isMetaFunction,
            CompileSyntax getter)
            : base(token)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        FunctionSyntax(FunctionSyntax other, ParsedSyntax[] parts)
            : base(other, parts)
        {
            Getter = other.Getter;
            Setter = other.Setter;
            IsImplicit = other.IsImplicit;
            IsMetaFunction = other.IsMetaFunction;
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
        internal override CompileSyntax SurroundCompileSyntax(params ParsedSyntax[] parts)
            => new FunctionSyntax(this, parts);

        internal IContextMetaFunctionFeature ContextMetaFunctionFeature(CompoundView compoundView)
        {
            if(!IsMetaFunction)
                return null;
            NotImplementedMethod(compoundView);
            return null;
        }

        internal IMetaFunctionFeature MetaFunctionFeature(CompoundView compoundView)
        {
            if(!IsMetaFunction)
                return null;
            NotImplementedMethod(compoundView);
            return null;
        }

        internal IFunctionFeature FunctionFeature(CompoundView compoundView)
        {
            if(IsMetaFunction)
                return null;
            return new FunctionBodyType(compoundView, this);
        }

        [DisableDump]
        protected override IEnumerable<Syntax> SyntaxChildren
        {
            get
            {
                yield return Getter;
                yield return Setter;
            }
        }
    }
}