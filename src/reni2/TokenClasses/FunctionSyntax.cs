using System.Linq;
using System.Collections.Generic;
using System;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
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

        public FunctionSyntax(SourcePart token, CompileSyntax setter, bool isImplicit, bool isMetaFunction, CompileSyntax getter)
            : base(token)
        {
            Getter = getter;
            Setter = setter;
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        string Tag
        {
            get
            {
                return (IsMetaFunction ? "{0}{0}" : "{0}")
                    .ReplaceArgs("/{0}\\")
                    .ReplaceArgs(IsImplicit ? "!" : "");
            }
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return context
                .FindRecentCompoundView
                .FunctionalType(this)
                .Result(category);
        }

        protected override bool GetIsLambda() { return true; }

        internal override string DumpPrintText
        {
            get
            {
                return
                    (Setter == null ? "" : Setter.DumpPrintText)
                        + Tag
                        + (Getter == null ? "" : Getter.DumpPrintText)
                    ;
            }
        }

        protected override string GetNodeDump()
        {
            var getter = Getter == null ? "" : "(" + Getter.NodeDump + ")";
            var setter = Setter == null ? "" : "(" + Setter.NodeDump + ")";
            return setter + base.GetNodeDump() + getter;
        }

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
    }
}

