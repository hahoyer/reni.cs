using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.TokenClasses
{
    sealed class BraceMatchDumper : DumpableObject
    {
        readonly SourceSyntax _parent;
        readonly int _depth;
        public BraceMatchDumper(SourceSyntax parent, int depth)
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

        internal string Token => _parent.Token.Characters.NodeDump;

        BraceMatchDumper Dump(SourceSyntax sourceSyntax)
            => sourceSyntax == null
                ? null
                : new BraceMatchDumper(sourceSyntax, _depth - 1);
    }
}