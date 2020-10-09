using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(true, false)]
    [Variant(false, true)]
    sealed class Function : TokenClass, IValueProvider
    {
        readonly bool IsImplicit;
        readonly bool IsMetaFunction;

        public Function(bool isImplicit, bool isMetaFunction)
        {
            IsImplicit = isImplicit;
            IsMetaFunction = isMetaFunction;
        }

        public override string Id => TokenId(IsImplicit, IsMetaFunction);

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
            =>
                FunctionSyntax.Create(binaryTree.Left, IsImplicit, IsMetaFunction, binaryTree.Right, binaryTree
                    , scope);

        public static string TokenId(bool isImplicit = false, bool isMetaFunction = false)
            => "/" + (isImplicit? "!" : "") + "\\" + (isMetaFunction? "/\\" : "");
    }
}