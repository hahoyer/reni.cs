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
            => left.CreateDeclarationSyntax
                (token, new Validation.SyntaxError(IssueId.MissingValueInDeclaration, token));

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.CreateDeclarationSyntax(token, right);

        protected override Syntax Terminal(SourcePart token)
            => new DeclarationSyntax
                (token, new Validation.SyntaxError(IssueId.MissingValueInDeclaration, token));
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
    }

    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TerminalToken
    {
        protected override ReniParser.Syntax Terminal(SourcePart token)
            => new Syntax(this);

        [DisableDump]
        internal virtual bool IsKeyword => true;
        [DisableDump]
        internal virtual bool IsError => false;
        [DisableDump]
        internal virtual bool DeclaresMutable => false;
        [DisableDump]
        internal virtual bool DeclaresConverter => false;

        internal sealed class Syntax : ReniParser.Syntax
        {
            [EnableDump]
            internal readonly DeclarationTagToken Tag;

            internal Syntax(DeclarationTagToken tag) { Tag = tag; }

            [DisableDump]
            internal override bool IsKeyword => Tag.IsKeyword;
            [DisableDump]
            internal override bool IsError => Tag.IsError;

            [DisableDump]
            internal override CompileSyntax ToCompiledSyntax
            {
                get
                {
                    NotImplementedMethod();
                    return null;
                }
            }

            internal override ReniParser.Syntax ExclamationSyntax(SourcePart token)
                => new DeclarationTagSyntax(this, token);

            internal ReniParser.Syntax DeclarationSyntax(SourcePart token, CompileSyntax body)
                => new DeclarationSyntax(token, body, DefinableSyntax(token, null));

            internal DefinableSyntax DefinableSyntax(SourcePart token, Definable definable)
                => new DefinableSyntax(token, definable, Tag);
        }
    }

    sealed class DeclarationTagSyntax : Syntax
    {
        [EnableDump]
        DeclarationTagToken.Syntax Tag { get; }
        SourcePart Token { get; }

        public DeclarationTagSyntax(DeclarationTagToken.Syntax tag, SourcePart token)
        {
            Tag = tag;
            Token = token;
        }

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
            => IssueId.UnexpectedDeclarationTag.Syntax(Token, Tag).ToCompiledSyntax;

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => Tag.DeclarationSyntax(token, right.ToCompiledSyntax);

        internal override Syntax SuffixedBy(Definable definable, SourcePart token)
            => Tag.DefinableSyntax(token, definable);
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