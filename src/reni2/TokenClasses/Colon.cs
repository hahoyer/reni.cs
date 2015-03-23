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
        internal Syntax DeclarationSyntax(SourcePart token, CompileSyntax body)
            =>
                new DeclarationSyntax
                    (
                    token,
                    body,
                    new DefinableTokenSyntax(new DeclarationTagSyntax(this), token)
                    );

        protected override Syntax Terminal(SourcePart token)
            => new DeclarationTagSyntax(this);

        [DisableDump]
        internal virtual bool IsKeyword => false;
        [DisableDump]
        internal virtual bool IsError => false;
    }

    sealed class ConverterToken : DeclarationTagToken
    {
        const string TokenId = "converter";
        public override string Id => TokenId;
        internal override bool IsKeyword => true;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken
    {
        const string TokenId = "mutable";
        public override string Id => TokenId;
        internal override bool IsKeyword => true;
    }

    sealed class DeclarationTagSyntax : Syntax
    {
        [EnableDump]
        readonly DeclarationTagToken _tag;

        internal DeclarationTagSyntax(DeclarationTagToken tag) { _tag = tag; }

        [DisableDump]
        internal bool DeclaresMutable => _tag is MutableDeclarationToken;
        [DisableDump]
        internal bool DeclaresConverter => _tag is ConverterToken;
        [DisableDump]
        internal override bool IsKeyword => _tag.IsKeyword;
        [DisableDump]
        internal override bool IsError => _tag.IsError;

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => _tag.DeclarationSyntax(token, right.ToCompiledSyntax);

        internal override Syntax SuffixedBy(Definable definable, SourcePart token)
            => new DefinableTokenSyntax(definable, token, this);
    }
}