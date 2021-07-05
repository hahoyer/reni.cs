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
        readonly Syntax Parent;
        readonly BinaryTree ParentDefinedHeadAnchor;
        readonly Reni.SyntaxTree.Syntax Main;

        readonly Configuration Configuration;

        [EnableDump(Order = -1)]
        readonly Formatter Formatter;

        bool IsIndentRequired;

        Syntax LeftNeighbor;

        [UsedImplicitly]
        Syntax[] ChildrenForDebug;

        Syntax
        (
            BinaryTree headAnchor,
            Reni.SyntaxTree.Syntax main, Configuration configuration
            , Syntax parent
        )
        {
            ParentDefinedHeadAnchor = headAnchor;
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

        BinaryTree HeadAnchor => this.CachedValue(() => ParentDefinedHeadAnchor ?? Formatter.GetHeadAnchor(Main));

        [EnableDump(Order = -4)]
        string MainPosition => Main?.Position;

        [EnableDump(Order = -3)]
        string HeadAnchorPosition => HeadAnchor?.Token.SourcePart().GetDumpAroundCurrent(5);

        int IndentDirection => IsIndentRequired? 1 : 0;

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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Syntax[] Children => this.CachedValue(GetChildren);

        ISourcePartEdit[] HeadAnchorEdits
            => (HeadAnchor ?? Formatter.GetHeadAnchor(Main)).GetWhiteSpaceEdits(Configuration, 0)
                .ToArray();

        ISourcePartEdit[] ChildrenEdits
            => Children
                .SelectMany(GetChildEdits)
                .ToArray();

        [EnableDump(Order = -5)]
        int LineBreakCount
            => this.CachedValue(()
                => Parent == null || !Parent.IsLineSplit || LeftNeighbor == null
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
            ChildrenForDebug = result;
            return result;
        }

        internal static Syntax Create(Reni.SyntaxTree.Syntax target, Configuration configuration)
            => new(null, target, configuration, null);

        Syntax Create(Formatter.Child child)
            => new(child.HeadAnchor, child.FlatItem, Configuration, this);

        ISourcePartEdit[] GetEdits()
        {
            var sourcePartEdits = T(HeadAnchorEdits, ChildrenEdits)
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