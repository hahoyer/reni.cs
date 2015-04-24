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
        internal readonly Exclamation.Syntax[] Tags;

        internal ExclamationSyntaxList(Exclamation.Syntax[] tags, SourcePart token)
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
        readonly Definable _definable;
        readonly ExclamationSyntaxList _exclamationSyntaxList;

        internal DeclaratorSyntax(Definable definable, ExclamationSyntaxList exclamationSyntaxList)
        {
            _definable = definable;
            _exclamationSyntaxList = exclamationSyntaxList;
        }

        internal override Checked<Syntax> CreateDeclarationSyntax(SourcePart token, Syntax right)
            => DeclarationSyntax.Create
                (
                right,
                _definable,
                _exclamationSyntaxList.Tags
                );

        protected override IEnumerable<Syntax> DirectChildren
        {
            get { yield return _exclamationSyntaxList; }
        }
    }
}