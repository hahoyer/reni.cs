using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    /// <summary>
    ///     Static syntax items
    /// </summary>
    abstract class Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer
    {
        internal abstract class NoChildren : Syntax
        {
            protected NoChildren(BinaryTree target)
                : base(target) { }

            protected NoChildren(int objectId, BinaryTree target)
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

        internal virtual Result<ValueSyntax> ToValueSyntax(BinaryTree target= null)
        {
            if(GetType().Is<ValueSyntax>())
                return (ValueSyntax)this;

            NotImplementedMethod(target);
            return default;
        }

        internal Result<CompoundSyntax> ToCompoundSyntax(BinaryTree target = null)
            => this is CompoundSyntax compoundSyntax
                ? compoundSyntax
                : ToCompoundSyntaxHandler(target);

        protected virtual Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target= null)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal virtual DeclarationSyntax[] ToDeclarationsSyntax(BinaryTree target = null)
        {
            NotImplementedMethod(target);
            return default;
        }
    }
}