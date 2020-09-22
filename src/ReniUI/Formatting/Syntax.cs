using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    public sealed class Syntax : ValueCache.IContainer
    {
        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        readonly Syntax Parent;
        internal readonly Reni.TokenClasses.Syntax Target;
        internal readonly Syntax Left;
        internal readonly Syntax Right;

        public Syntax(Reni.TokenClasses.Syntax target, Syntax parent)
        {
            Target = target;
            Parent = parent;
            if(Target.Left != null)
                Left = new Syntax(Target.Left, this);
            if(Target.Right != null)
                Right = new Syntax(Target.Right, this);
        }

        Syntax LeftParent =>
            Parent != null && Parent.Target.Left == Target
                ? Parent.LeftParent
                : Parent;

        Syntax RightParent =>
            Parent != null && Parent.Target.Right == Target
                ? Parent.RightParent
                : Parent;

        internal SourcePart MainToken => Target.Token.Characters;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> MainWhiteSpaces => Target.Token.PrecededWith;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> LeftWhiteSpaces => Target.Left == null ? null : MainWhiteSpaces;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> RightWhiteSpaces =>
            Right?.LeftMost.MainWhiteSpaces;

        [DisableDump]
        Syntax LeftMost => Left?.LeftMost ?? this;

        [DisableDump]
        Syntax RightMost => Right?.RightMost ?? this;

        [DisableDump]
        Reni.TokenClasses.Syntax LeftNeighbor => (Left?.RightMost ?? LeftParent).Target;

        [DisableDump]
        Reni.TokenClasses.Syntax RightNeighbor => (Right?.LeftMost ?? RightParent).Target;

        [DisableDump]
        internal ITokenClass TokenClass => Target.TokenClass;

        internal bool LeftSideSeparator()
        {
            var left = LeftNeighbor?.TokenClass;
            return !LeftWhiteSpaces.HasComment() && SeparatorExtension.Get(left, Target.TokenClass);
        }

        internal bool RightSideSeparator()
        {
            var tokenClass = RightNeighbor?.TokenClass;
            return tokenClass != null &&
                   !RightWhiteSpaces.HasComment() &&
                   SeparatorExtension.Get(Target.TokenClass, tokenClass);
        }
    }
}