using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass, IBelongingsMatcher, IStatementsProvider
    {
        public static string TokenId(int level) => ",;.".Substring(level, 1);

        [DisableDump]
        internal readonly int Level;

        public List(int level) { Level = level; }

        public override string Id => TokenId(Level);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;

        public Result<Statement[]> Get(List type, Syntax left, SourcePart token, Syntax right)
        {
            if(type != null && type != this)
                return null;

            var leftStatements = CreateStatements(left, token);
            var rightStatements = CreateStatements(right, token);
            var target = leftStatements?.Target.plus(rightStatements?.Target);
            var issues = leftStatements?.Issues.plus(rightStatements?.Issues);
            return new Result<Statement[]>(target, issues);
        }

        Result<Statement[]> CreateStatements(Syntax syntax, SourcePart token)
            => syntax == null
                ? Statement.CreateStatements(token, new EmptyList(token))
                : syntax.GetStatements(this);
    }
}