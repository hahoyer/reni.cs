using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Syntax : SyntaxView<Syntax>
    {
        class CacheContainer
        {
            internal ValueCache<SplitItem> SplitItem;
            internal ValueCache<SplitMaster> SplitMaster;
        }

        readonly CacheContainer Cache = new CacheContainer();
        readonly Configuration Configuration;
        bool ForceLineSplit => Parent?.IsLineSplit ?? false;

        bool IsIndentRequired;

        internal Syntax(Reni.SyntaxTree.Syntax flatItem, Configuration configuration)
            : this(flatItem, new PositionDictionary<Syntax>(), 0, null)
            => Configuration = configuration;

        Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context, int index, Syntax parent)
            : base(flatItem, parent, context)
        {
            if(parent != null)
                Configuration = parent.Configuration;
        }

        bool IsLineSplit
        {
            get
            {
                var master = FlatItem.MainAnchor;
                if(master.TokenClass is EndOfText || master.TokenClass is BeginOfText)
                    return false;
                return ForceLineSplit || GetHasAlreadyLineBreakOrIsTooLong(master);
            }
        }

        [EnableDump]
        new Reni.SyntaxTree.Syntax FlatItem => base.FlatItem;

        [EnableDump]
        [EnableDumpExcept(null)]
        string ParentToken => Parent?.Anchors.DumpSource();

        [EnableDump(Order = 10)]
        string[] Children => FlatItem
            .Children
            .Select(node => node?.Anchor.SourceParts.DumpSource())
            .ToArray();

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IEnumerable<ISourcePartEdit> Edits
        {
            get
            {
                var trace = ObjectId == -240;
                StartMethodDump(trace);
                try
                {
                    var result = EditGroups
                        .ToArray()
                        .Indent(IndentDirection);
                    Dump(nameof(result), result);
                    //Tracer.Assert(CheckMultilineExpectations(result), Anchor.Dump);

                    Tracer.ConditionalBreak(trace);
                    return ReturnMethodDump(result, trace);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IndentDirection IndentDirection => IsIndentRequired? IndentDirection.ToRight : IndentDirection.NoIndent;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<ISourcePartEdit> EditGroups
        {
            get
            {
                var anchorEdits
                    = FlatItem.Anchor.Items
                        .Select(node
                            =>
                            (
                                position: node.Token.Characters.Position
                                , data: GetMainEdits(node)
                            )
                        )
                        .ToArray();

                var childEdits
                    = DirectChildren
                        .Where(node => node != null)
                        .Select(node
                            =>
                            (
                                position: node.FlatItem.MainAnchor.Token.Characters.Position
                                , data: node.Edits ?? new ISourcePartEdit[0]
                            )
                        )
                        .ToArray();

                return anchorEdits
                    .Concat(childEdits)
                    .OrderBy(node => node.position)
                    .SelectMany(node => node.data);
            }
        }

        bool GetHasAlreadyLineBreakOrIsTooLong(BinaryTree target)
        {
            var basicLineLength = target.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, index, this);

        IEnumerable<ISourcePartEdit> GetMainEdits(BinaryTree target)
        {
            var result = GetWhiteSpaceEdits(target);
            
            if(IsLineSplit)
            {
                var isBelongingTo = target.Right?.TokenClass.IsBelongingTo(FlatItem.MainAnchor.TokenClass) ?? false;
                result = result.Concat(T(GetLineSplitter(target, isBelongingTo))).ToArray();
            }

            return result;
        }

        IEnumerable<ISourcePartEdit> GetWhiteSpaceEdits(BinaryTree target)
        {
            if(target.Token.PrecededWith.Any())
                return T(new WhiteSpaceView(target.Token.PrecededWith, Configuration, target.IsSeparatorRequired));
            return T(new EmptyWhiteSpaceView(target.Token.Characters.Start, target.IsSeparatorRequired));
        }

        bool AdditionalLineBreaksForMultilineItems;
        
        ISourcePartEdit GetLineSplitter(BinaryTree target, bool isInsideChain)
        {
            var second = target.Right;
            if(isInsideChain)
                second = second.Left;

            if(!AdditionalLineBreaksForMultilineItems || second == null)
                return SourcePartEditExtension.MinimalLineBreak;

            if(GetHasAlreadyLineBreakOrIsTooLong(target.Left) || GetHasAlreadyLineBreakOrIsTooLong(second))
                return SourcePartEditExtension.MinimalLineBreaks;

            return SourcePartEditExtension.MinimalLineBreak;
        }

    }
}