using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace Reni.Helper
{
    public abstract class SyntaxWithParent : DumpableObject, ValueCache.IContainer
    {
        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        [DisableDump]
        public readonly SyntaxWithParent Parent;

        [DisableDump]
        public readonly TokenClasses.Syntax Target;

        [DisableDump]
        public SyntaxWithParent Left
            => Target.Left == null ? null : this.CachedValue(() => Create(Target.Left, this));

        [DisableDump]
        public SyntaxWithParent Right
            => Target.Right == null ? null : this.CachedValue(() => Create(Target.Right, this));

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
        public SourcePart MainToken => Target.Token.Characters;

        [EnableDumpExcept(null)]
        public IEnumerable<IItem> MainWhiteSpaces => Target.Token.PrecededWith;

        [EnableDumpExcept(null)]
        public IEnumerable<IItem> LeftWhiteSpaces
            => Target.Left == null ? null : MainWhiteSpaces;

        [EnableDumpExcept(null)]
        public IEnumerable<IItem> RightWhiteSpaces =>
            Right?.LeftMost.MainWhiteSpaces;

        [DisableDump]
        SyntaxWithParent LeftMost => Left?.LeftMost ?? this;

        [DisableDump]
        SyntaxWithParent RightMost => Right?.RightMost ?? this;

        [DisableDump]
        public TokenClasses.Syntax LeftNeighbor => (Left?.RightMost ?? LeftParent).Target;

        [DisableDump]
        public TokenClasses.Syntax RightNeighbor => (Right?.LeftMost ?? RightParent).Target;

        [DisableDump]
        public ITokenClass TokenClass => Target.TokenClass;
    }

    public abstract class SyntaxWithParent<TSyntax> : SyntaxWithParent
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

        protected sealed override SyntaxWithParent Create(TokenClasses.Syntax target, SyntaxWithParent parent)
            => Create(target, (TSyntax) parent);

        protected abstract TSyntax Create(TokenClasses.Syntax target, TSyntax parent);
    }
}