using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using JetBrains.Annotations;
using Reni.Helper;
using Reni.SyntaxTree;
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
                StopByObjectIds(244);
            }

            [UsedImplicitly]
            internal string TargetPosition => Target.Token.SourcePart().GetDumpAroundCurrent(5);

            internal static Anchor Create(BinaryTree anchor) => anchor == null ? null : new Anchor(anchor);

            internal ISourcePartEdit[] GetWhiteSpaceEdits(Configuration configuration)
                => Target.GetWhiteSpaceEdits(configuration, LineBreakCount);

            internal void EnsureLineBreaks(int count)
            {
                if (LineBreakCount < count)
                    LineBreakCount = count;
            }
        }

        internal sealed class AnchorsClass : DumpableObject
        {
            [EnableDump]
            [EnableDumpExcept(null)]
            internal Anchor Prefix;

            [EnableDump]
            [EnableDumpExcept(null)]
            internal Anchor Begin;

            [EnableDump]
            [EnableDumpExcept(null)]
            internal Anchor End;

            internal ISourcePartEdit[] GetEdits(Configuration configuration)
                => T(Prefix, Begin, End)
                    .Select(anchor => anchor?.GetWhiteSpaceEdits(configuration))
                    .ConcatMany()
                    .ToArray();
        }

        [DisableDump]
        internal readonly Configuration Configuration;

        [EnableDump]
        internal readonly AnchorsClass Anchors = new();


        [DisableDump]
        readonly Syntax Parent;

        readonly IItem Main;

        [EnableDump(Order = -1)]
        readonly Formatter Formatter;

        readonly bool HasAdditionalIndent;

        Syntax LeftNeighbor;

        [UsedImplicitly]
        Syntax[] ChildrenForDebug;

        Syntax(Formatter.Child child, Configuration configuration, Syntax parent)
        {
            Main = child.FlatItem;
            Configuration = configuration;
            Anchors.Prefix = Anchor.Create(child.PrefixAnchor);
            HasAdditionalIndent = child.HasAdditionalIndent;
            Parent = parent;
            Formatter = child.Formatter;
            if (Main != null)
            {
                var frameAnchors = Formatter.GetFrameAnchors(Main);
                Anchors.Begin = Anchor.Create(frameAnchors.begin);
                Anchors.End = Anchor.Create(frameAnchors.end);
            }

            StopByObjectIds();
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        int ITree<Syntax>.DirectChildCount => Children.Length;

        Syntax ITree<Syntax>.GetDirectChild(int index) => Children[index];

        int ITree<Syntax>.LeftDirectChildCount => 0;

        bool IsIndentRequired => Formatter.IsIndentRequired;

        [EnableDump(Order = -3)]
        string MainPosition => (Main as Reni.SyntaxTree.Syntax)?.Position;

        int IndentDirection => IsIndentRequired ? 1 : 0;

        [EnableDump(Order = 3)]
        [EnableDumpExcept(false)]
        internal bool IsLineSplit => HasAlreadyLineBreakOrIsTooLong;

        bool HasAlreadyLineBreakOrIsTooLong
        {
            get
            {
                if (Main == null)
                    return false;
                var lineLength = Main.Anchor.Main.GetFlatLength(Configuration.EmptyLineLimit != 0);
                return lineLength == null || lineLength > Configuration.MaxLineLength;
            }
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ISourcePartEdit[] Edits => GetEdits();

        [EnableDump(Order = 4)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal Syntax[] Children => this.CachedValue(GetChildren);

        ISourcePartEdit[] AnchorEdits => Anchors.GetEdits(Configuration);

        ISourcePartEdit[] ChildrenEdits
            => Children
                .SelectMany(GetChildEdits)
                .Indent(IndentDirection)
                .ToArray();

        internal static Syntax Create(Reni.SyntaxTree.Syntax target, Configuration configuration)
            => new(new(null, target, false), configuration, null);

        Syntax Create(Formatter.Child child) => new(child, Configuration, this);

        Syntax[] GetChildren()
        {
            var result = Formatter.GetChildren(Main).Select(Create).ToArray();
            for (var index = 0; index < result.Length - 1; index++)
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
            if (child == null)
                return new ISourcePartEdit[0];
            var result = child.Edits;
            if (IsLineSplit && child.HasAdditionalIndent && !(child.IsLineSplit && child.Formatter.IsIndentRequired))
                result = result.Indent(1).ToArray();
            return result;
        }

        internal void SetupLineBreaks()
        {
            Formatter.SetupUnconditionalLineBreaks(this);

            if (!IsLineSplit)
                return;

            Formatter.SetupLineBreaks(this);
            foreach (var child in Children)
                child.SetupLineBreaks();
        }

        internal void EnsureLineBreaks(int count, bool beforePrefix)
        {
            if (beforePrefix && Anchors.Prefix != null)
                Anchors.Prefix.EnsureLineBreaks(count);
            else if (Anchors.Begin != null)
                Anchors.Begin.EnsureLineBreaks(count);
            else
            {
                (Children.Length > 0).Assert();
                Children[0].EnsureLineBreaks(count, true);
            }
        }
    }
}