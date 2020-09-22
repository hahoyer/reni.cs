using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class BraceMatchDumper : DumpableObject
    {
        readonly Syntax _parent;
        readonly int _depth;

        public BraceMatchDumper(Syntax parent, int depth)
            : base(null)
        {
            _parent = parent;
            _depth = depth;
        }

        protected override string Dump(bool isRecursion)
            => _depth > 0 ? base.Dump(isRecursion) : "...";

        protected override string GetNodeDump() => _parent.NodeDump;

        internal BraceMatchDumper Left => Dump(_parent.Left);

        internal ITokenClass TokenClass => _parent.TokenClass;

        internal BraceMatchDumper Right => Dump(_parent.Right);

        internal string Token => _parent.Option.MainToken.NodeDump;

        BraceMatchDumper Dump(Syntax syntax)
            => syntax == null
                ? null
                : new BraceMatchDumper(syntax, _depth - 1);
    }
}