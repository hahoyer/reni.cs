using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(true, false)]
    [Variant(false, true)]
    sealed class Function : TokenClass, IValueToken
    {
        internal readonly bool IsImplicit;
        internal readonly bool IsMetaFunction;

        public Function(bool isImplicit, bool isMetaFunction)
        {
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        public override string Id => TokenId(IsImplicit, IsMetaFunction);

        IValueProvider IValueToken.Provider => Factory.Function;

        public static string TokenId(bool isImplicit = false, bool isMetaFunction = false)
            => "@" + (isMetaFunction? "@" : "")+ (isImplicit? "!" : "") ;
    }
}