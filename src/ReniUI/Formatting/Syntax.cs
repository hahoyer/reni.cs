using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Syntax : DumpableObject, ValueCache.IContainer, ITree<Syntax>
    {
        readonly Reni.SyntaxTree.Syntax Main;

        readonly Configuration Configuration;

        readonly Syntax Parent;

        [EnableDump(Order = -1)]
        readonly Formatter Formatter;

        bool IsIndentRequired;

        Syntax LeftNeighbor;

        Syntax
        (
            Reni.SyntaxTree.Syntax main, Configuration configuration
            , Syntax parent
        )
        {
            Main = main;
            Configuration = configuration;
            Parent = parent;
            Formatter = Formatter.Create(Main);
            StopByObjectIds();
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();
        int ITree<Syntax>.DirectChildCount => Children.Length;
        Syntax ITree<Syntax>.GetDirectChild(int index) => Children[index];
        int ITree<Syntax>.LeftDirectChildCount => 0;

        [EnableDump(Order = -4)]
        string MainPosition => Main?.Position;

        BinaryTree[] FrameAnchors => Formatter.GetFrameAnchors(Main);
        int IndentDirection => IsIndentRequired? 1 : 0;
        [EnableDump(Order = -3)]
        string FramePosition => FrameAnchors?.SourceParts().DumpSource();

        [EnableDump(Order = 3)]
        [EnableDumpExcept(false)]
        bool IsLineSplit => HasAlreadyLineBreakOrIsTooLong;

        bool HasAlreadyLineBreakOrIsTooLong
        {
            get
            {
                var childrenInformation = Children.Any(child => child.HasAlreadyLineBreakOrIsTooLong);
                if(childrenInformation)
                    return true;

                var basicLineLength =
                    Main == null? 0 : Main.MainAnchor.GetFlatLength(Configuration.EmptyLineLimit != 0);
                return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
            }
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ISourcePartEdit[] Edits => GetEdits();

        [EnableDump(Order = 4)]
        Syntax[] Children => this.CachedValue(GetChildren);

        ISourcePartEdit[] FrameAnchorEdits
            => FrameAnchors
                .SelectMany(node => node.GetWhiteSpaceEdits(Configuration, 0))
                .ToArray();

        ISourcePartEdit[] ChildrenEdits
            => Children
                .SelectMany(GetChildEdits)
                .ToArray();

        [EnableDump(Order = -5)]
        int LineBreakCount
            => this.CachedValue(()
                => Parent == null || !Parent.IsLineSplit || LeftNeighbor == null || Main == null
                    ? 0
                    : Configuration.AdditionalLineBreaksForMultilineItems &&
                    (LeftNeighbor.HasAlreadyLineBreakOrIsTooLong || HasAlreadyLineBreakOrIsTooLong)
                        ? 2
                        : 1);

        Syntax[] GetChildren()
        {
            var result = Formatter.GetChildren(Main).Select(Create).ToArray();
            for(var index = 0; index < result.Length - 1; index++)
                result[index + 1].LeftNeighbor = result[index];
            return result;
        }

        internal static Syntax Create(Reni.SyntaxTree.Syntax target, Configuration configuration)
            => new(target, configuration, null);

        Syntax Create(Formatter.Child child)
            => new(child.FlatItem, Configuration, this);

        ISourcePartEdit[] GetEdits()
        {
            var sourcePartEdits = T(FrameAnchorEdits, ChildrenEdits)
                .ConcatMany().ToArray();
            return sourcePartEdits
                .AddLineBreaks(LineBreakCount)
                .Indent(IndentDirection)
                .ToArray();
        }

        IEnumerable<ISourcePartEdit> GetChildEdits(Syntax child)
        {
            if(child == null)
                return new ISourcePartEdit[0];

            var result = child.Edits;

            if(IsLineSplit && child.LeftNeighbor != null && Formatter.IsIndentAtTailRequired)
                result = result.Indent(1).ToArray();
            return result;
        }
    }
}