using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class ExclamationSyntaxList : NonCompileSyntax
    {
        internal static ExclamationSyntaxList Create(SourcePart token)
            => new ExclamationSyntaxList(new Exclamation.Syntax[0], token);

        internal readonly Exclamation.Syntax[] Tags;

        ExclamationSyntaxList(Exclamation.Syntax[] tags, SourcePart token)
            : base(token)
        {
            Tags = tags;
        }

        internal ExclamationSyntaxList(Exclamation.Syntax item, SourcePart token)
            : this(new[] {item}, token) {}

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => Tags;

        internal override Checked<Syntax> SuffixedBy(Definable definable, SourcePart token)
            => new DeclaratorSyntax(definable, this);

        internal override Checked<Syntax> CreateDeclarationSyntax(SourcePart token, Syntax right)
            => DeclarationSyntax.Create(right, null, Tags);

    }

    sealed class DeclaratorSyntax : CompileSyntax
    {
        readonly Definable Definable;
        readonly ExclamationSyntaxList ExclamationSyntaxList;

        internal DeclaratorSyntax(Definable definable, ExclamationSyntaxList exclamationSyntaxList)
        {
            Definable = definable;
            ExclamationSyntaxList = exclamationSyntaxList;
        }

        internal override Checked<Syntax> CreateDeclarationSyntax(SourcePart token, Syntax right)
            => DeclarationSyntax.Create
                (
                    right,
                    Definable,
                    ExclamationSyntaxList.Tags
                );

        protected override IEnumerable<Syntax> DirectChildren
        {
            get { yield return ExclamationSyntaxList; }
        }
    }
}