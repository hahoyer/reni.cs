using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";
        public override string Id => TokenId;

        protected override Checked<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => left.CreateDeclarationSyntax
                (
                    token,
                    new EmptyList(token),
                    IssueId.MissingValueInDeclaration.CreateIssue(token)
                );

        protected override Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token);

        protected override Checked<OldSyntax> OldInfix
            (OldSyntax left, SourcePart token, OldSyntax right)
            => left.CreateDeclarationSyntax(token, right);

        protected override Checked<Value> Infix(Syntax left, SourcePart token, Value right)
        {
            var declaration = left.Declarator;
            var item = declaration.Value.Statement(token, right);
            var result = CompoundSyntax.Create(item.Value);
            return new Checked<Value>
                (result.Value, declaration.Issues.plus(item.Issues).plus(result.Issues));
        }
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
        Checked<DeclaratorTags> IDeclaratorTagProvider.Get
            (Syntax left, SourcePart token, Syntax right)
        {
            if(left == null && right == null)
                return new DeclaratorTags(this, token);

            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    sealed class ConverterToken : DeclarationTagToken
    {
        const string TokenId = "converter";
        public override string Id => TokenId;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken
    {
        const string TokenId = "mutable";
        public override string Id => TokenId;
    }

    sealed class MixInDeclarationToken : DeclarationTagToken
    {
        const string TokenId = "mix_in";
        public override string Id => TokenId;
    }
}