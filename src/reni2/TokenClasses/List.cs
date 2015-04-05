using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level) => ",;.".Substring(level, 1);
        readonly int _level;
        public List(int level) { _level = level; }

        public override string Id => TokenId(_level);

        protected override Checked<Syntax> Terminal(SourcePart token)
            =>
                ListSyntax
                    (
                        new EmptyList(),
                        new EmptyList());

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => ListSyntax(new EmptyList(), right);

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => ListSyntax(left, new EmptyList());

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => ListSyntax(left, right);

        internal override string Reformat
            (SourceSyntax target, IFormattingConfiguration configuration)
            => configuration.List(Id, CollectTokens(target), CollectItems(target, configuration));

        IEnumerable<string> CollectItems
            (SourceSyntax target, IFormattingConfiguration configuration)
        {
            do
            {
                yield return target.Left?.Reformat(configuration);
                target = target.Right;
            } while(target != null && target.TokenClass == this);
            yield return target?.Reformat(configuration);
        }

        IEnumerable<WhiteSpaceToken[]> CollectTokens(SourceSyntax target)
        {
            do
            {
                yield return target.Token.PrecededWith;
                target = target.Right;
            } while (target != null && target.TokenClass == this);
        }

        ListSyntax ListSyntax(Syntax left, Syntax right)
            => new ListSyntax(this, left.ToList(this).Concat(right.ToList(this)).ToArray());

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;
    }
}