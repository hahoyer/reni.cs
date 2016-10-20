using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class EndOfText : TokenClass, IDefaultScopeProvider, IBracketMatch<Syntax>
    {
        const string TokenId = PrioTable.EndOfText;
        [DisableDump]
        public override string Id => TokenId;
        [DisableDump]
        internal override bool IsVisible => false;
        bool IDefaultScopeProvider.MeansPublic => true;
        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();

        sealed class Matched : DumpableObject, IParserTokenType<Syntax>
        {
            Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(left.Right == null);
                Tracer.Assert(left.Left.Left == null);
                return left.Left.Right;
            }

            string IParserTokenType<Syntax>.PrioTableId => "()";
        }
    }

    sealed class BeginOfText : TokenClass
    {
        const string TokenId = PrioTable.BeginOfText;
        [DisableDump]
        public override string Id => TokenId;
    }
}