using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Syntax : DumpableObject, ValueCache.IContainer, ITree<Syntax>
    {
        [DisableDump]
        internal readonly Reni.SyntaxTree.Syntax Main;

        readonly Configuration Configuration;

        [EnableDump(Order = -1)]
        readonly BinaryTree Head;

        readonly Syntax Parent;
        readonly Formatter Formatter;

        bool IsIndentRequired;

        [EnableDump]
        SourcePosition Position;

        Syntax
        (
            SourcePosition position, BinaryTree head, Reni.SyntaxTree.Syntax main, Configuration configuration
            , Syntax parent
        )
        {
            Position = position;
            Head = head;
            Main = main;
            Configuration = configuration;
            Parent = parent;
            Formatter = Formatter.Create(Main);
            StopByObjectIds();
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<Syntax>.DirectChildCount => Children.Length;
        Syntax ITree<Syntax>.GetDirectChild(int index) => Children[index];
        int ITree<Syntax>.LeftDirectChildCount => 0;

        BinaryTree[] Anchors => Formatter.GetAnchors(this);
        int IndentDirection => IsIndentRequired? 1 : 0;

        [EnableDump(Order = -2)]
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
                var mainLength = Head?.Token.Characters.Length ?? 0;
                return basicLineLength == null || basicLineLength + mainLength > Configuration.MaxLineLength;
            }
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ISourcePartEdit[] Edits => GetEdits();

        [EnableDump]
        Syntax[] Children => this.CachedValue(() => GetChildren().Select(Create).ToArray());

        ISourcePartEdit[] ChildEdits
            => GetChildEdits().ConcatMany().ToArray();

        ISourcePartEdit[] AnchorEdits
            => Anchors
                .SelectMany(node => node.GetWhiteSpaceEdits(Configuration))
                .ToArray();

        ISourcePartEdit[] GetEdits(int lineBreakCount = 0)
            => T(Head.GetWhiteSpaceEdits(Configuration, lineBreakCount), AnchorEdits, ChildEdits)
                .ConcatMany()
                .Indent(IndentDirection)
                .ToArray();

        int GetLineBreakCount(bool? leftLines, bool? rightLines)
            => Parent == null || !Parent.IsLineSplit || leftLines == null
                ? 0
                : !Configuration.AdditionalLineBreaksForMultilineItems ||
                  rightLines == null ||
                  !(leftLines.Value || rightLines.Value)
                    ? 1
                    : 2;

        IEnumerable<ISourcePartEdit[]> GetChildEdits()
        {
            bool? leftLineBreaks = null;
            foreach(var child in Children)
            {
                var rightLineBreaks = child?.HasAlreadyLineBreakOrIsTooLong;

                if(child != null)
                {
                    var lineBreakCount = child.GetLineBreakCount(leftLineBreaks, rightLineBreaks);
                    yield return child.GetEdits(lineBreakCount).ToArray();
                }

                leftLineBreaks = rightLineBreaks;
            }
        }

        IEnumerable<Formatter.Child> GetChildren() => Formatter.GetChildren(Main);

        internal static Syntax Create(Reni.SyntaxTree.Syntax target, Configuration configuration)
            => new Syntax(null, null, target, configuration, null);

        Syntax Create(Formatter.Child child)
            => new Syntax(child.Position, child.Head, child.FlatItem, Configuration, this);
    }
}