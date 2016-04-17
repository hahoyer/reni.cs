using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : SpecialSyntax
    {
        internal static Result<Parser.Value> Create
            (Syntax left, bool isImplicit, bool isMetaFunction, SourcePart token, Syntax right)
        {
            var leftvalue = left?.Value;
            var rightvalue = right?.Value;
            var target = new FunctionSyntax
                (leftvalue?.Target, isImplicit, isMetaFunction, token, rightvalue?.Target);
            var issues = leftvalue?.Issues.plus(rightvalue?.Issues);
            return new Result<Parser.Value>(target, issues);
        }

        internal Parser.Value Getter { get; }
        SourcePart Token { get; }
        internal Parser.Value Setter { get; }

        bool IsMetaFunction { get; }
        internal bool IsImplicit { get; }

        FunctionSyntax
            (
            Parser.Value setter,
            bool isImplicit,
            bool isMetaFunction,
            SourcePart token,
            Parser.Value getter)
        {
            Token = token;
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        [DisableDump]
        protected override IEnumerable<Parser.Value> DirectChildren
        {
            get
            {
                yield return Setter;
                yield return Getter;
            }
        }

        internal override SourcePosn SourceStart => Setter?.SourceStart ?? Token.Start;
        internal override SourcePosn SourceEnd => Getter?.SourceEnd ?? Token.End;

        internal string Tag
            => (IsMetaFunction ? "{0}{0}" : "{0}")
                .ReplaceArgs("/{0}\\")
                .ReplaceArgs(IsImplicit ? "!" : "");

        internal override Result ResultForCache(ContextBase context, Category category)
            => context
                .FindRecentCompoundView
                .FunctionalType(this)
                .Result(category);

        protected override bool GetIsLambda() => true;

        internal override ResultCache.IResultProvider FindSource
            (IContextReference ext, ContextBase context)
        {
            var result = Getter.ResultCache
                .Where(item => item.Value.Exts.Contains(ext))
                .Where(item => (item.Key as Child)?.Parent == context)
                .ToArray();
            if(result.Any())
                return result.First().Value.Provider;

            result = Setter.ResultCache
                .Where(item => item.Value.Exts.Contains(ext))
                .Where(item => (item.Key as Child)?.Parent == context)
                .ToArray();
            return result.Any() ? result.First().Value.Provider : null;
        }

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