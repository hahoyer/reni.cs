using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass, IStatementProvider
    {
        public const string TokenId = ":";
        public override string Id => TokenId;

        Result<Statement> IStatementProvider.Get(Syntax left, Syntax right)
            => left
                .Declarator
                .Convert(x => x.Statement(right.Value));
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : ScannerTokenClass, ISubParser<Syntax>, IType<Syntax>
    {
        public const string TokenId = "!";

        readonly ISubParser<Syntax> Parser;

        public Exclamation(ISubParser<Syntax> parser) { Parser = parser; }

        IType<Syntax> ISubParser<Syntax>.Execute
            (SourcePosn sourcePosn, Stack<OpenItem<Syntax>> stack)
            => Parser.Execute(sourcePosn, stack);

        public override string Id => TokenId;

        Syntax IType<Syntax>.Create
            (Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        string IType<Syntax>.PrioTableId => TokenId;
    }


    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TerminalToken, IDeclaratorTagProvider, IDeclarationTag
    {
        Result<Declarator> IDeclaratorTagProvider.Get
            (Syntax syntax)
        {
            if(syntax.Left== null && syntax.Right == null)
                return new Declarator(new IDeclarationTag[] {this}, null);

            NotImplementedMethod( syntax);
            return null;
        }

        public static IEnumerable<string> DeclarationOptions
        {
            get
            {
                yield return ConverterToken.TokenId;
                yield return MutableDeclarationToken.TokenId;
                yield return MixInDeclarationToken.TokenId;
            }
        }
    }

    sealed class ConverterToken : DeclarationTagToken
    {
        internal const string TokenId = "converter";
        public override string Id => TokenId;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "mutable";
        public override string Id => TokenId;
    }

    sealed class MixInDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "mix_in";
        public override string Id => TokenId;
    }
}