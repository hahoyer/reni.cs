using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class ReassignToken : Definable
    {
        public const string Id = ":=";

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }

    sealed class EnableReassignToken : TokenClass
    {
        public const string Id = "<:=>";
        protected override Syntax Prefix(SourcePart token, Syntax right)
        {
            return new EnableReassignSyntax(token, right.ToCompiledSyntax);
        }
    }

    sealed class EnableReassignSyntax : CompileSyntax
    {
        readonly CompileSyntax _target;
        public EnableReassignSyntax(SourcePart token, CompileSyntax target)
            : base(token)
        {
            _target = target;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _target.ObtainResult(context, category);
        }
    }
}