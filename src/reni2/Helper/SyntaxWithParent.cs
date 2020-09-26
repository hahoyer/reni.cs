using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.Helper
{
    abstract class SyntaxWithParent : DumpableObject, ValueCache.IContainer
    {
        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        [DisableDump]
        public readonly SyntaxWithParent Parent;

        [DisableDump]
        public readonly TokenClasses.Syntax Target;

        protected SyntaxWithParent(TokenClasses.Syntax target, SyntaxWithParent parent)
        {
            Target = target;
            Parent = parent;
        }

        protected abstract SyntaxWithParent Create(TokenClasses.Syntax target, SyntaxWithParent parent);

        [DisableDump]
        SyntaxWithParent LeftParent =>
            Parent != null && Parent.Target.Left == Target
                ? Parent.LeftParent
                : Parent;

        [DisableDump]
        SyntaxWithParent RightParent =>
            Parent != null && Parent.Target.Right == Target
                ? Parent.RightParent
                : Parent;

        [DisableDump]
        internal SyntaxWithParent Left
            => Target.Left == null ? null : this.CachedValue(() => Create(Target.Left, this));

        [DisableDump]
        internal SyntaxWithParent Right
            => Target.Right == null ? null : this.CachedValue(() => Create(Target.Right, this));

        [DisableDump]
        internal IToken Token => Target.Token;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> WhiteSpaces => Token.PrecededWith;

        [DisableDump]
        internal SourcePart SourcePart => Target.SourcePart;

        [DisableDump]
        internal SyntaxWithParent LeftMost => Left?.LeftMost ?? this;

        [DisableDump]
        internal SyntaxWithParent RightMost => Right?.RightMost ?? this;

        [DisableDump]
        internal SyntaxWithParent LeftNeighbor => (Left?.RightMost ?? LeftParent);

        [DisableDump]
        internal SyntaxWithParent RightNeighbor => (Right?.LeftMost ?? RightParent);
        
        [DisableDump]
        internal bool IsLeftChild => Parent?.Left == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.Right == this;
        
        [EnableDump]
        internal ITokenClass TokenClass => Target.TokenClass;

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;

    }

    abstract class SyntaxWithParent<TSyntax> : SyntaxWithParent
        where TSyntax : SyntaxWithParent<TSyntax>
    {
        protected SyntaxWithParent(TokenClasses.Syntax target, TSyntax parent)
            : base(target, parent) {}

        [DisableDump]
        public new TSyntax Parent => (TSyntax) base.Parent;

        [DisableDump]
        public new TSyntax Left => (TSyntax) base.Left;

        [DisableDump]
        public new TSyntax Right => (TSyntax) base.Right;

        [DisableDump]
        public new TSyntax LeftMost => (TSyntax) base.LeftMost;

        [DisableDump]
        public new TSyntax RightMost => (TSyntax) base.RightMost;

        [DisableDump]
        public new TSyntax LeftNeighbor => (TSyntax) base.LeftNeighbor ;

        [DisableDump]
        public new TSyntax RightNeighbor => (TSyntax) base.RightNeighbor;

        protected sealed override SyntaxWithParent Create(TokenClasses.Syntax target, SyntaxWithParent parent)
            => Create(target, (TSyntax) parent);

        protected abstract TSyntax Create(TokenClasses.Syntax target, TSyntax parent);
    }
}