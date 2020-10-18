using System;
using System.Linq;
using Reni.Context;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Helper
{
    sealed class Syntax : PairView<Syntax>
    {
        internal Syntax(BinaryTree binary, Syntax parent = null, Func<Reni.SyntaxTree.Syntax> getFlatSyntax = null)
            : base(binary, parent, getFlatSyntax) { }

        internal string[] GetDeclarationOptions(ContextBase context)
            => (Syntax.FlatItem as ValueSyntax)?.GetDeclarationOptions(context).ToArray();

        protected override Syntax Create(BinaryTree flatItem) => new Syntax(flatItem, this);

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem)
            => new Syntax(null, this, () => flatItem);
    }
}