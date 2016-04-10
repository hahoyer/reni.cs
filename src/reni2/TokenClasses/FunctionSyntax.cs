using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
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
        public static Parser.Value
            Create(Parser.Value setter, bool isImplicit, bool isMetaFunction, Parser.Value getter) 
            => new FunctionSyntax(setter, isImplicit, isMetaFunction, getter);

        internal Parser.Value Getter { get; }

        internal Parser.Value Setter { get; }

        bool IsMetaFunction { get; }
        internal bool IsImplicit { get; }

        public FunctionSyntax
            (
            Parser.Value setter,
            bool isImplicit,
            bool isMetaFunction,
            Parser.Value getter)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren
        {
            get
            {
                yield return Setter;
                yield return Getter;
            }
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