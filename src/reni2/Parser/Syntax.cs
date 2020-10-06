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
        internal abstract class Terminal : Syntax
        {
            protected Terminal(BinaryTree target)
                : base(target) { }

            protected Terminal(int objectId, BinaryTree target)
                : base(objectId, target) { }

            protected sealed override IEnumerable<Syntax> GetChildren() => new Syntax[0];
        }

        internal readonly BinaryTree Target;

        protected Syntax(BinaryTree target) => Target = target;

        protected Syntax(int objectId, BinaryTree target)
            : base(objectId)
            => Target = target;

        internal Syntax[] Children => this.CachedValue(() => GetChildren().ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        Syntax ITree<Syntax>.Child(int index) => Children[index];
        int ITree<Syntax>.ChildrenCount => Children.Length;
        protected abstract IEnumerable<Syntax> GetChildren();

        internal virtual Result<ValueSyntax> ToValueSyntax(BinaryTree target)
        {
            NotImplementedMethod(target);
            return default;
        }
    }
}