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
    sealed class List : TokenClass, IBelongingsMatcher, IStatementsProvider, SyntaxFactory.IDeclarationsToken
    {
        [DisableDump]
        internal readonly int Level;

        public List(int level) => Level = level;

        public override string Id => TokenId(Level);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;

        [Obsolete("", true)]
        Result<Statement[]> IStatementsProvider.Get(List type, BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(type != null && type != this)
                return null;

            var leftStatements = CreateStatements(binaryTree.Left, binaryTree, scope);
            var rightStatements = CreateStatements(binaryTree.Right, binaryTree, scope);
            var target = leftStatements?.Target.plus(rightStatements?.Target);
            var issues = leftStatements?.Issues.plus(rightStatements?.Issues);
            return new Result<Statement[]>(target, issues);
        }

        SyntaxFactory.IDeclarationsProvider SyntaxFactory.IDeclarationsToken.Provider => SyntaxFactory.List;
        public static string TokenId(int level) => ",;.".Substring(level, 1);

        [Obsolete("", true)]
        Result<Statement[]> CreateStatements(BinaryTree binaryTree, BinaryTree parent, ISyntaxScope scope)
            => binaryTree == null
                ? Statement.CreateStatements(new EmptyList(parent), scope.DefaultScopeProvider)
                : binaryTree.GetStatements(scope, this);
    }
}