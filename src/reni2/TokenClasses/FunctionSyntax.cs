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
        public readonly CompileSyntax Getter;
        readonly bool _isImplicit;
        readonly bool _isMetaFunction;
        public readonly CompileSyntax Setter;

        public FunctionSyntax(SourcePart token, CompileSyntax setter, bool isImplicit, bool isMetaFunction, CompileSyntax getter)
            : base(token)
        {
            Getter = getter;
            Setter = setter;
            _isImplicit = isImplicit;
            _isMetaFunction = isMetaFunction;
        }

        string Tag
        {
            get
            {
                return (_isMetaFunction ? "{0}{0}" : "{0}")
                    .ReplaceArgs("/{0}\\")
                    .ReplaceArgs(IsImplicit ? "!" : "");
            }
        }

        internal override bool IsImplicit { get { return _isImplicit; } }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return context
                .FindRecentContainerView
                .UniqueFunctionalType(this)
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

        internal IContextMetaFunctionFeature ContextMetaFunctionFeature(ContainerView containerView)
        {
            if(!_isMetaFunction)
                return null;
            NotImplementedMethod(containerView);
            return null;
        }

        internal IMetaFunctionFeature MetaFunctionFeature(ContainerView containerView)
        {
            if(!_isMetaFunction)
                return null;
            NotImplementedMethod(containerView);
            return null;
        }

        internal IFunctionFeature FunctionFeature(ContainerView containerView)
        {
            if(_isMetaFunction)
                return null;
            return new FunctionBodyType(containerView, this);
        }
    }
}

