using System.Configuration;
using hw.DebugFormatter;
using Reni.Helper;
using Reni.TokenClasses;

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
            : base(flatItem, parent, context, index)
        {

            var c = parent?.ToString();

            (!(Parent?.TokenClass is IRightBracket) || flatItem.Anchor.TokenClass is ILeftBracket)
                .Assert(flatItem.Dump);
            
        }

        [EnableDump]
        new Reni.SyntaxTree.Syntax FlatItem => base.FlatItem;

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, index, this);
    }
}