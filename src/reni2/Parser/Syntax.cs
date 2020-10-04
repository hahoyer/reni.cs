using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.TokenClasses;

namespace Reni.Parser
{
    /// <summary>
    ///     Static syntax items
    /// </summary>
    abstract class Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer
    {
        internal readonly BinaryTree BinaryTree;

        protected Syntax(BinaryTree binaryTree) => BinaryTree = binaryTree;

        protected Syntax(int objectId, BinaryTree binaryTree)
            : base(objectId)
            => BinaryTree = binaryTree;

        internal Syntax[] Children => this.CachedValue(() => GetChildren().ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        Syntax ITree<Syntax>.Child(int index) => Children[index];
        int ITree<Syntax>.ChildrenCount => Children.Length;
        protected abstract IEnumerable<Syntax> GetChildren();
    }
}