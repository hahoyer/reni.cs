using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";
        public override string Id => TokenId;

        protected override Checked<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            =>
                left.CreateDeclarationSyntax
                    (
                        token,
                        new EmptyList(token),
                        IssueId.MissingValueInDeclaration.CreateIssue(token)
                    );

        protected override Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token);

        protected override Checked<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => left.CreateDeclarationSyntax(token, right);

        protected override Checked<OldSyntax> OldTerminal(SourcePart token)
            => new DeclarationSyntax(new EmptyList(token), null)
                .Issues(IssueId.MissingValueInDeclaration.CreateIssue(token));
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : ScannerTokenClass, ISubParser<Syntax> , IType<Syntax>
    {
        public const string TokenId = "!";

        readonly ISubParser<TokenClasses.Syntax> Parser;

        public Exclamation(ISubParser<TokenClasses.Syntax> parser) { Parser = parser; }

        IType<TokenClasses.Syntax> ISubParser<TokenClasses.Syntax>.Execute
            (SourcePosn sourcePosn, Stack<OpenItem<TokenClasses.Syntax>> stack)
            => Parser.Execute(sourcePosn, stack);

        public override string Id => TokenId;

        internal sealed class Syntax : Parser.OldSyntax
        {
            [EnableDump]
            internal DeclarationTagToken.Syntax Tag { get; }
            internal override SourcePart Token { get; }

            public Syntax(DeclarationTagToken.Syntax tag, SourcePart token)
            {
                Tag = tag;
                Token = token;
            }

            [DisableDump]
            internal override Checked<Value> ToCompiledSyntax
            {
                get
                {
                    var result = IssueId.UnexpectedDeclarationTag.Syntax(Token, Tag);
                    var value = result.Value.ToCompiledSyntax;
                    return new Checked<Value>(value.Value, result.Issues.plus(value.Issues));
                }
            }
        }

        TokenClasses.Syntax IType<TokenClasses.Syntax>.Create(TokenClasses.Syntax left, IToken token, TokenClasses.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;

        }

        string IType<TokenClasses.Syntax>.PrioTableId => TokenId;
    }


    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TerminalToken
    {
        protected override Checked<Parser.OldSyntax> OldTerminal(SourcePart token)
            => new Syntax(this);

        [DisableDump]
        internal virtual bool DeclaresMutable => false;
        [DisableDump]
        internal virtual bool DeclaresConverter => false;
        [DisableDump]
        internal virtual bool DeclaresMixIn => false;

        internal sealed class Syntax : Parser.OldSyntax
        {
            internal readonly DeclarationTagToken Tag;

            internal Syntax(DeclarationTagToken tag) { Tag = tag; }

            [DisableDump]
            internal override Checked<Value> ToCompiledSyntax
            {
                get
                {
                    NotImplementedMethod();
                    return null;
                }
            }

            internal override Checked<ExclamationSyntaxList> ExclamationSyntax(SourcePart token)
                => new ExclamationSyntaxList(new Exclamation.Syntax(this, token), token);
        }
    }

    sealed class ConverterToken : DeclarationTagToken
    {
        const string TokenId = "converter";
        public override string Id => TokenId;
        [DisableDump]
        internal override bool DeclaresConverter => true;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken
    {
        const string TokenId = "mutable";
        public override string Id => TokenId;
        [DisableDump]
        internal override bool DeclaresMutable => true;
    }

    sealed class MixInDeclarationToken : DeclarationTagToken
    {
        const string TokenId = "mix_in";
        public override string Id => TokenId;
        [DisableDump]
        internal override bool DeclaresMixIn => true;
    }
}