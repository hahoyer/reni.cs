using Reni.Helper;

namespace ReniUI.Helper
{
    sealed class Syntax : SyntaxView<Syntax>
    {
        internal Syntax
        (
            Reni.SyntaxTree.Syntax flatItem,
            PositionDictionary<Syntax> context,
            int index = 0,
            Syntax parent = null
        )
            : base(flatItem, parent, context, index) { }

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, index, this);
    }
}