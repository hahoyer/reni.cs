using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(true, false)]
    [Variant(false, true)]
    sealed class Function : TokenClass, SyntaxFactory.IValueToken
    {
        internal readonly bool IsImplicit;
        internal readonly bool IsMetaFunction;

        public Function(bool isImplicit, bool isMetaFunction)
        {
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        public override string Id => TokenId(IsImplicit, IsMetaFunction);

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Function;

        public static string TokenId(bool isImplicit = false, bool isMetaFunction = false)
            => "@" + (isMetaFunction? "@" : "")+ (isImplicit? "!" : "") ;
    }
}