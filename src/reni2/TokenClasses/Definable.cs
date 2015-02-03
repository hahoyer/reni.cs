﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass
    {
        protected override sealed Syntax Terminal(SourcePart token) => new DefinableTokenSyntax(this, token, false);

        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new ExpressionSyntax(this, null, token, right.ToCompiledSyntax);

        protected override sealed Syntax Suffix(Syntax left, SourcePart token) => left.SuffixedBy(this, token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new ExpressionSyntax(this, left.ToCompiledSyntax, token, right.ToCompiledSyntax);

        [DisableDump]
        protected string DataFunctionName => Name.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable();
    }

    sealed class ConcatArrays : Definable, ITokenClassWithId
    {
        public const string Id = "<<";
        public const string MutableId = "<<:=";
        internal bool IsMutable { get; }

        public ConcatArrays(bool isMutable) { IsMutable = isMutable; }

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);

        string ITokenClassWithId.Id => IsMutable ? MutableId : Id;
    }

    sealed class ArrayAccess : Definable, ITokenClassWithId
    {
        public const string Id = ">>";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    sealed class Reference: Definable, ITokenClassWithId
    {
        public const string Id = "reference";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }
}