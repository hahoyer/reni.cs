using System;
using hw.DebugFormatter;
using hw.Parser;
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

        [Obsolete("",true)]
        Result<Statement[]> IStatementsProvider.Get
            (List type, Syntax syntax, IValuesScope scope)
        {
            if(type != null && type != this)
                return null;

            var leftStatements = CreateStatements(syntax.Left, syntax, scope);
            var rightStatements = CreateStatements(syntax.Right, syntax, scope);
            var target = leftStatements?.Target.plus(rightStatements?.Target);
            var issues = leftStatements?.Issues.plus(rightStatements?.Issues);
            return new Result<Statement[]>(target, issues);
        }

        [Obsolete("",true)]
        Result<Statement[]> CreateStatements
            (Syntax syntax, Syntax parent, IValuesScope scope)
            => syntax == null
                ? Statement.CreateStatements(new EmptyList(parent), scope.DefaultScopeProvider)
                : syntax.GetStatements(scope, this);
    }
}