using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Helper
{
    sealed class Syntax : BinaryTreeSyntaxWithParent<Syntax>
    {
        internal Syntax(BinaryTree binary, Syntax parent = null, Func<Reni.Parser.Syntax> getFlatSyntax = null)
            : base(binary, parent, getFlatSyntax) { }

        internal new BinaryTree FlatItem => base.FlatItem;

        [DisableDump]
        internal IEnumerable<Issue> Issues => FlatItem.Issues;

        internal string[] GetDeclarationOptions(ContextBase context)
            => (FlatSyntax as ValueSyntax)?.GetDeclarationOptions(context).ToArray();

        protected override Syntax Create(BinaryTree flatItem) => new Syntax(flatItem, this);

    }

    abstract class ProxySyntax : Reni.Parser.Syntax.NoChildren
    {
        internal class ColonLevel : ProxySyntax
        {
            public ColonLevel(Syntax client, BinaryTree target)
                : base(client, target) { }
        }

        internal class ListLevel : ProxySyntax
        {
            public ListLevel(Syntax client, BinaryTree target)
                : base(client, target) { }
        }

        internal class LeftBracketOfRightBracket : ProxySyntax
        {
            public LeftBracketOfRightBracket(Syntax client, BinaryTree target)
                : base(client, target) { }
        }

        [DisableDump]
        internal readonly Syntax Client;

        ProxySyntax(Syntax client, BinaryTree target)
            : base(target)
            => Client = client;
    }
}