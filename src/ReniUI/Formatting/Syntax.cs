using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using JetBrains.Annotations;
using Reni.Helper;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Syntax : DumpableObject, ValueCache.IContainer, ITree<Syntax>
    {
        internal sealed class Anchor : DumpableObject
        {
            [DisableDump]
            readonly BinaryTree Target;

            [EnableDump]
            [EnableDumpExcept(0)]
            int LineBreakCount;

            Anchor(BinaryTree target)
            {
                Target = target;
                StopByObjectIds(243);
            }

            internal string TargetPosition => Target.Token.SourcePart().GetDumpAroundCurrent(5);

            internal static Anchor Create(BinaryTree anchor) => anchor == null? null : new Anchor(anchor);

            internal ISourcePartEdit[] GetWhiteSpaceEdits(Configuration configuration)
                => Target.GetWhiteSpaceEdits(configuration, LineBreakCount);

            internal void EnsureLineBreaks(int count)
            {
                if(LineBreakCount < count)
                    LineBreakCount = count;
            }
        }

        [EnableDump(Order = -2)]
        [EnableDumpExcept(null)]
        internal readonly Anchor EndAnchor;

        [DisableDump]
        internal readonly Configuration Configuration;

        [EnableDump(Order = -4)]
        [EnableDumpExcept(null)]
        readonly Anchor BeginAnchor;

        [DisableDump]
        readonly Syntax Parent;

        readonly Reni.SyntaxTree.Syntax Main;

        [EnableDump(Order = -1)]
        readonly Formatter Formatter;

        [EnableDump]
        [EnableDumpExcept(null)]
        readonly Anchor PrefixAnchor;

        bool IsIndentRequired => Formatter.IsIndentRequired;

        Syntax LeftNeighbor;

        [UsedImplicitly]
        Syntax[] ChildrenForDebug;

        Syntax(Formatter.Child child, Configuration configuration, Syntax parent)
        {
            Main = child.FlatItem;
            Configuration = configuration;
            PrefixAnchor = Anchor.Create(child.PrefixAnchor);
            Parent = parent;
            Formatter = Formatter.Create(Main);
            var frameAnchors = Formatter.GetFrameAnchors(Main);
            BeginAnchor = Anchor.Create(frameAnchors.begin);
            EndAnchor = Anchor.Create(frameAnchors.end);

            StopByObjectIds();
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        int ITree<Syntax>.DirectChildCount => Children.Length;
        Syntax ITree<Syntax>.GetDirectChild(int index) => Children[index];
        int ITree<Syntax>.LeftDirectChildCount => 0;

        [EnableDump(Order = -3)]
        string MainPosition => Main?.Position;

        int IndentDirection => IsIndentRequired? 1 : 0;

        [EnableDump(Order = 3)]
        [EnableDumpExcept(false)]
        internal bool IsLineSplit => HasAlreadyLineBreakOrIsTooLong;

        bool HasAlreadyLineBreakOrIsTooLong
        {
            get
            {
                var lineLength = Main.MainAnchor.GetFlatLength(Configuration.EmptyLineLimit != 0);
                return lineLength == null || lineLength > Configuration.MaxLineLength;
            }
        }

        int ClosingLineBreakCount => IsLineSplit? Formatter.GetClosingLineBreakCount(this) : 0;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ISourcePartEdit[] Edits => GetEdits();

        [EnableDump(Order = 4)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal Syntax[] Children => this.CachedValue(GetChildren);

        ISourcePartEdit[] AnchorEdits
            => T(PrefixAnchor, BeginAnchor, EndAnchor)
                .Select(anchor => anchor?.GetWhiteSpaceEdits(Configuration))
                .ConcatMany()
                .ToArray();

        ISourcePartEdit[] ChildrenEdits
            => Children
                .SelectMany(GetChildEdits)
                .Indent(IndentDirection)
                .ToArray();

        internal static Syntax Create(Reni.SyntaxTree.Syntax target, Configuration configuration)
            => new(new Formatter.Child(null, target), configuration, null);

        Syntax Create(Formatter.Child child) => new(child, Configuration, this);

        Syntax[] GetChildren()
        {
            var result = Formatter.GetChildren(Main).Select(Create).ToArray();
            for(var index = 0; index < result.Length - 1; index++)
                result[index + 1].LeftNeighbor = result[index];
            ChildrenForDebug = result;
            return result;
        }

        ISourcePartEdit[] GetEdits()
            => T(AnchorEdits, ChildrenEdits)
                .ConcatMany()
                .ToArray();

        IEnumerable<ISourcePartEdit> GetChildEdits(Syntax child)
        {
            if(child == null)
                return new ISourcePartEdit[0];
            var result = child.Edits;
            if(IsLineSplit && child.LeftNeighbor != null && Formatter.IsIndentAtTailRequired)
                result = result.Indent(1).ToArray();
            return result;
        }

        internal void SetupLineBreaks()
        {
            Formatter.SetupUnconditionalLineBreaks(this);

            if(!IsLineSplit)
                return;

            Formatter.SetupLineBreaks(this);
            foreach(var child in Children)
                child.SetupLineBreaks();
        }

        internal void EnsureLineBreaks(int count, bool beforePrefix)
        {
            if(beforePrefix && PrefixAnchor != null)
                PrefixAnchor.EnsureLineBreaks(count);
            else if(BeginAnchor != null)
                BeginAnchor.EnsureLineBreaks(count);
            else
            {
                (Children.Length > 0).Assert();
                Children[0].EnsureLineBreaks(count, true);
            }
        }
    }
}