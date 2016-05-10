using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level)
            => level == 0 ? PrioTable.BeginOfText : "{[(".Substring(level - 1, 1);

        public LeftParenthesis(int level) { Level = level; }
        [DisableDump]
        internal int Level { get; }
        [DisableDump]
        public override string Id => TokenId(Level);
        [DisableDump]
        internal override bool IsVisible => Level != 0;

        [DisableDump]
        internal bool IsFrameToken => Level == 0;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as RightParenthesis)?.Level == Level;
    }
}