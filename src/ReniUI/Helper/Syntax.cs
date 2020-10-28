using System.Linq;
using hw.DebugFormatter;
using Reni.Helper;
using Reni.Parser;

namespace ReniUI.Helper
{
    public sealed class Syntax : SyntaxView<Syntax>
    {
        internal Syntax
        (
            Reni.SyntaxTree.Syntax flatItem,
            PositionDictionary<Syntax> context,
            int index = 0,
            Syntax parent = null
        )
            : base(flatItem, parent, context, index) { }

        [EnableDump]
        new Reni.SyntaxTree.Syntax FlatItem => base.FlatItem;

        [EnableDump]
        [EnableDumpExcept(null)]
        string ParentToken => Parent?.SourcePart.DumpSource(5);

        [EnableDump(Order = 10)]
        string[] Children => FlatItem
            .Children
            .Select(node => node?.Anchor.SourcePart.DumpSource(5))
            .ToArray();

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, index, this);
    }
}