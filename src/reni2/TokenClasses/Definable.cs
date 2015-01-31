using System;
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
        protected override sealed Syntax Terminal(SourcePart token) { return new DefinableTokenSyntax(this, token); }

        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
        {
            return new ExpressionSyntax(this, null, token, right.ToCompiledSyntax);
        }

        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
        {
            return new ExpressionSyntax(this, left.ToCompiledSyntax, token, null);
        }

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            return new ExpressionSyntax(this, left.ToCompiledSyntax, token, right.ToCompiledSyntax);
        }

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }

        [DisableDump]
        internal virtual IEnumerable<IGenericProviderForDefinable> Genericize { get { return this.GenericListFromDefinable(); } }
    }

    sealed class ConcatArrays : Definable
    {
        public static readonly string Id = "<<";
        public static readonly string MutableId = "<<:=";
        internal bool IsMutable { get; }

        public ConcatArrays(bool isMutable) { IsMutable = isMutable; }

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    sealed class ArrayAccess : Definable
    {
        public static readonly string Id = ">>";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}