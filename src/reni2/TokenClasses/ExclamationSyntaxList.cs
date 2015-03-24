using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class ExclamationSyntaxList : NonCompileSyntax
    {
        internal readonly Exclamation.Syntax[] Tags;
        internal new readonly SyntaxError[] Issues;

        ExclamationSyntaxList(Exclamation.Syntax[] tags, SyntaxError[] issues, SourcePart token)
            : base(token)
        {
            Tags = tags;
            Issues = issues;
        }

        internal ExclamationSyntaxList(Exclamation.Syntax item, SourcePart token)
            : this(new[] {item}, new SyntaxError[0], token) {}

        [DisableDump]
        internal override bool IsKeyword => true;

        internal override Syntax SuffixedBy(Definable definable, SourcePart token)
            => new DeclaratorSyntax(definable, this);

        internal ExclamationSyntaxList AddError(SyntaxError syntaxError)
            => new ExclamationSyntaxList(Tags, Issues.plus(syntaxError), Token);

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => new DeclarationSyntax
                (
                Issues,
                right.ToCompiledSyntax,
                null,
                Tags.Select(item => item.Tag.Tag).ToArray());

        protected override IEnumerable<Syntax> DirectChildren => Tags.plus<Syntax>(Issues);
    }

    sealed class DeclaratorSyntax : CompileSyntax
    {
        readonly Definable _definable;
        readonly ExclamationSyntaxList _exclamationSyntaxList;

        internal DeclaratorSyntax(Definable definable, ExclamationSyntaxList exclamationSyntaxList)
        {
            _definable = definable;
            _exclamationSyntaxList = exclamationSyntaxList;
        }

        internal override bool IsIdentifier => true;

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => new DeclarationSyntax
                (
                _exclamationSyntaxList.Issues,
                right.ToCompiledSyntax,
                _definable,
                _exclamationSyntaxList.Tags.Select(item => item.Tag.Tag).ToArray()
                );

        protected override IEnumerable<Syntax> DirectChildren
        {
            get { yield return _exclamationSyntaxList; }
        }
    }
}