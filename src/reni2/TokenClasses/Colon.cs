using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            =>
                left.CreateDeclarationSyntax
                    (token, new EmptyList(), IssueId.MissingValueInDeclaration.CreateIssue(token));

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token);

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => left.CreateDeclarationSyntax(token, right);

        protected override Checked<Syntax> Terminal(SourcePart token)
            => new DeclarationSyntax(new EmptyList(), null).Issues
                (IssueId.MissingValueInDeclaration.CreateIssue(token));
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : ScannerTokenClass, ISubParser<SourceSyntax>
    {
        public const string TokenId = "!";

        readonly ISubParser<SourceSyntax> _parser;

        public Exclamation(ISubParser<SourceSyntax> parser) { _parser = parser; }

        IType<SourceSyntax> ISubParser<SourceSyntax>.Execute
            (SourcePosn sourcePosn, Stack<OpenItem<SourceSyntax>> stack)
            => _parser.Execute(sourcePosn, stack);

        public override string Id => TokenId;

        internal sealed class Syntax : Parser.Syntax
        {
            [EnableDump]
            internal DeclarationTagToken.Syntax Tag { get; }
            SourcePart Token { get; }

            public Syntax(DeclarationTagToken.Syntax tag, SourcePart token)
            {
                Tag = tag;
                Token = token;
            }

            [DisableDump]
            internal override Checked<CompileSyntax> ToCompiledSyntax
            {
                get
                {
                    var result = IssueId.UnexpectedDeclarationTag.Syntax(Token, Tag);
                    var value = result.Value.ToCompiledSyntax;
                    return new Checked<CompileSyntax>(value.Value, result.Issues.plus(value.Issues));
                }
            }
        }
    }


    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TerminalToken
    {
        protected override Checked<Parser.Syntax> Terminal(SourcePart token)
            => new Syntax(this);

        [DisableDump]
        internal virtual bool DeclaresMutable => false;
        [DisableDump]
        internal virtual bool DeclaresConverter => false;

        internal sealed class Syntax : Parser.Syntax
        {
            internal readonly DeclarationTagToken Tag;

            internal Syntax(DeclarationTagToken tag) { Tag = tag; }

            [DisableDump]
            internal override Checked<CompileSyntax> ToCompiledSyntax
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
}