using System;
using System.Linq;
using hw.DebugFormatter;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Helper
{
    sealed class Syntax : PairView<Syntax>
    {
        internal Syntax(BinaryTree binary, Syntax parent = null, Func<Reni.Parser.Syntax> getFlatSyntax = null)
            : base(binary, parent, getFlatSyntax) { }

        internal string[] GetDeclarationOptions(ContextBase context)
            => (Syntax.FlatItem as ValueSyntax)?.GetDeclarationOptions(context).ToArray();

        protected override Syntax Create(BinaryTree flatItem) => new Syntax(flatItem, this);
        protected override Syntax Create(Reni.Parser.Syntax flatItem)
            => new Syntax(null, this, ()=>flatItem);
    }

    abstract class ProxySyntax : Reni.Parser.Syntax.NoChildren
    {
        internal class ColonLevel : ProxySyntax
        {
            public ColonLevel(Syntax client, BinaryTree anchor)
                : base(client, anchor) { }
        }

        internal class ListLevel : ProxySyntax
        {
            public ListLevel(Syntax client, BinaryTree anchor)
                : base(client, anchor) { }
        }

        internal class LeftBracketOfRightBracket : ProxySyntax
        {
            public LeftBracketOfRightBracket(Syntax client, BinaryTree anchor)
                : base(client, anchor) { }
        }

        [DisableDump]
        internal readonly Syntax Client;

        ProxySyntax(Syntax client, BinaryTree anchor)
            : base(anchor)
            => Client = client;
    }
}