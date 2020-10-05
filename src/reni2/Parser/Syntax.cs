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
        internal readonly BinaryTree Target;

        protected Syntax(BinaryTree binaryTree) => Target = binaryTree;

        protected Syntax(int objectId, BinaryTree target)
            : base(objectId)
            => Target = target;

        internal Syntax[] Children => this.CachedValue(() => GetChildren().ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        Syntax ITree<Syntax>.Child(int index) => Children[index];
        int ITree<Syntax>.ChildrenCount => Children.Length;
        protected abstract IEnumerable<Syntax> GetChildren();
    }
}