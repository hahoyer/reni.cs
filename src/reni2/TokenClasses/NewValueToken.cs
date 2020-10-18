using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class NewValueToken : NonSuffixSyntaxToken
    {
        public const string TokenId = "new_value";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category)
            => context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);

        protected override Result Result
            (ContextBase context, Category category, ValueSyntax right, BinaryTree token)
        {
            NotImplementedMethod(context, category, token, right);
            return null;
        }
    }
}