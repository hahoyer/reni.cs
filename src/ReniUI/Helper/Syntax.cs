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
            Syntax parent = null
        )
            : base(flatItem, parent, context) { }

        [EnableDump]
        new Reni.SyntaxTree.Syntax FlatItem => base.FlatItem;

        [EnableDump]
        [EnableDumpExcept(null)]
        string ParentToken => Parent?.Anchors.DumpSource();

        [EnableDump(Order = 10)]
        string[] Children => FlatItem
            .Children
            .Select(node => node?.Anchor.SourceParts.DumpSource())
            .ToArray();

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, this);
    }
}