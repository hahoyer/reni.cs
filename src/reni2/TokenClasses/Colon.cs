using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";
        public override string Id => TokenId;

        protected override Syntax Suffix(Syntax left, SourcePart token)
            => left.CreateDeclarationSyntax(token, IssueId.MissingValueInDeclaration.Syntax(token));

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.CreateDeclarationSyntax(token, right);

        protected override Syntax Terminal(SourcePart token)
            => new DeclarationSyntax(IssueId.MissingValueInDeclaration.Syntax(token), null);
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

        internal sealed class Syntax : ReniParser.Syntax
        {
            [EnableDump]
            DeclarationTagToken.Syntax Tag { get; }
            SourcePart Token { get; }

            public Syntax(DeclarationTagToken.Syntax tag, SourcePart token)
            {
                Tag = tag;
                Token = token;
            }

            [DisableDump]
            internal override CompileSyntax ToCompiledSyntax
                => IssueId.UnexpectedDeclarationTag.Syntax(Token, Tag).ToCompiledSyntax;

            internal override ReniParser.Syntax CreateDeclarationSyntax
                (SourcePart token, ReniParser.Syntax right)
                => Tag.DeclarationSyntax(right.ToCompiledSyntax);
        }
    }

    sealed class ExclamationSyntaxList : Syntax
    {
        readonly Exclamation.Syntax[] _item;
        readonly SyntaxError[] _issue;

        ExclamationSyntaxList(Exclamation.Syntax[] item, SyntaxError[] issue)
        {
            _item = item;
            _issue = issue;
        }

        public ExclamationSyntaxList(Exclamation.Syntax item)
            : this(new[] {item}, new SyntaxError[0]) { }

        internal ExclamationSyntaxList AddError(SyntaxError syntaxError)
            => new ExclamationSyntaxList(_item, _issue.plus(syntaxError));

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

    }


    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TerminalToken
    {
        protected override ReniParser.Syntax Terminal(SourcePart token)
            => new Syntax(this);

        [DisableDump]
        internal virtual bool DeclaresMutable => false;
        [DisableDump]
        internal virtual bool DeclaresConverter => false;
        [DisableDump]
        internal virtual bool IsError => false;

        internal sealed class Syntax : ReniParser.Syntax
        {
            internal readonly DeclarationTagToken Tag;

            internal Syntax(DeclarationTagToken tag) { Tag = tag; }

            [DisableDump]
            internal override bool IsKeyword => true;

            [DisableDump]
            internal override CompileSyntax ToCompiledSyntax
            {
                get
                {
                    NotImplementedMethod();
                    return null;
                }
            }

            internal override ExclamationSyntaxList ExclamationSyntax(SourcePart token)
                => new ExclamationSyntaxList(new Exclamation.Syntax(this, token));

            internal ReniParser.Syntax DeclarationSyntax(CompileSyntax body)
                => new DeclarationSyntax(body, null, Tag);
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