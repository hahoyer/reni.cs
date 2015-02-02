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
using Reni.Type;

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
        protected override Syntax Prefix(SourcePart token, Syntax right)
            => new EnableReassignSyntax(token, right.ToCompiledSyntax);
        protected override Syntax Suffix(Syntax left, SourcePart token)
            => new EnableReassignTypeSyntax(left.ToCompiledSyntax, token);
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
        internal override bool IsMutableSyntax => true;
    }

    sealed class EnableReassignTypeSyntax : CompileSyntax
    {
        readonly CompileSyntax _target;
        public EnableReassignTypeSyntax(CompileSyntax target, SourcePart token)
            : base(token)
        {
            _target = target;
        }

        protected override string GetNodeDump() => "(" + _target.NodeDump + ")" + base.GetNodeDump();

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var leftType = context.Type(_target);
            var leftTypeType = ((TypeType) leftType).Value;
            return ((ArrayType) leftTypeType).Mutable.TypeType.Result(category);
        }
    }
}