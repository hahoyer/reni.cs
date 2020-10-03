﻿using System;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class EndOfText : TokenClass, IDefaultScopeProvider, IBracketMatch<Syntax>, IStatementsProvider
        , IValuesScope
    {
        sealed class Matched : DumpableObject, IParserTokenType<Syntax>
        {
            Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(left.Right == null);
                Tracer.Assert(left.Left.Left == null);
                return left;
            }

            string IParserTokenType<Syntax>.PrioTableId => "()";
        }

        const string TokenId = PrioTable.EndOfText;
        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new Matched();
        bool IDefaultScopeProvider.MeansPublic => true;

        [Obsolete("",true)]
        Result<Statement[]> IStatementsProvider.Get
            (List type, Syntax right, IValuesScope scope)
        {
            Tracer.Assert(type == null);
            Tracer.Assert(right != null);

            Tracer.Assert(right.Left != null);
            Tracer.Assert(right.TokenClass is EndOfText);
            Tracer.Assert(right.Right == null);

            Tracer.Assert(right.Left.Left == null);
            Tracer.Assert(right.Left.TokenClass is BeginOfText);

            Tracer.Assert(scope == null);

            var result = right.Left.Right;
            return result?.GetStatements(this) ?? new Result<Statement[]>(new Statement[0]);
        }

        [DisableDump]
        public override string Id => TokenId;

        [DisableDump]
        internal override bool IsVisible => false;

        IDefaultScopeProvider IValuesScope.DefaultScopeProvider => this;

        bool IValuesScope.IsDeclarationPart => false;
    }

    sealed class BeginOfText : TokenClass
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;
    }
}