using System.Linq;
using hw.DebugFormatter;
using Reni.Helper;
using Reni.Parser;

namespace ReniUI.Helper
{
    public sealed class Syntax : SyntaxView<Syntax>
    {
        internal Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context)
            : base(flatItem, null, context) { }

        Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context, Syntax parent)
            : base(flatItem, parent, context) { }

        [EnableDump]
        new Reni.SyntaxTree.Syntax FlatItem => base.FlatItem;

        [EnableDump(Order = -2)]
        [EnableDumpExcept(null)]
        string ParentDump => Parent?.ThisDump;

        [EnableDump(Order = -1)]
        [EnableDumpExcept(null)]
        string ThisDump => Anchors.DumpSource();

        [EnableDump(Order = 10)]
        string[] Children => FlatItem
            .Children
            .Select(node => node?.Anchor.SourceParts.DumpSource())
            .ToArray();

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, this);
    }
}