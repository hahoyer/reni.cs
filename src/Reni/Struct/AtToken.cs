using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Struct
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class AtToken : InfixPrefixSyntaxToken
    {
        public const string TokenId = "_A_T_";
        public override string Id => TokenId;

        protected override Result Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
        {
            var target = context.ResultAsReference(category.WithType, left);
            return target
                .Type
                .FindRecentCompoundView
                .AccessViaPositionExpression(category, right.Result(context))
                .ReplaceArg(target);
        }

        protected override Result Result(ContextBase context, Category category, ValueSyntax right)
            => context.FindRecentCompoundView.AtTokenResult(category, right.Result(context));
    }
}