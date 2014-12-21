using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Scanner;
using Reni.Feature;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class ReassignToken : Definable
    {
        public const string Id = ":=";

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    sealed class EnableReassignToken : TokenClass
    {
        public const string Id = ":=!";
        protected override Syntax Prefix(SourcePart token, Syntax right) => new EnableReassignSyntax(token, right.ToCompiledSyntax);
    }

    sealed class EnableReassignSyntax : Syntax
    {
        readonly CompileSyntax _target;
        public EnableReassignSyntax(SourcePart token, CompileSyntax target)
            : base(token)
        {
            _target = target;
        }

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + _target.NodeDump + ")";
        internal override CompileSyntax ContainerStatementToCompileSyntax => _target.ContainerStatementToCompileSyntax;
        internal override bool IsEnableReassignSyntax => true;
    }
}