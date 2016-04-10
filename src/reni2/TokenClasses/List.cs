using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level) => ",;.".Substring(level, 1);

        [DisableDump]
        internal readonly int Level;

        public List(int level) { Level = level; }

        public override string Id => TokenId(Level);

        protected override Checked<Value> GetValue(Syntax left, SourcePart token, Syntax right)
        {
            var leftResult = left?.Statements;
            var rightResult = right?.Statements;
            return new Checked<Value>(
                new CompoundSyntax(leftResult?.Value.plus(rightResult?.Value)),
                leftResult?.Issues.plus(rightResult?.Issues)
                );
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;
    }
}