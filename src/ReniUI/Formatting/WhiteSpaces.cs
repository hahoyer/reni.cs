using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.TokenClasses.Whitespace;

namespace ReniUI.Formatting
{
    /// <summary>
    ///     Encapsulates all comment, line-break and space formatting for a token.
    ///     This class has a very high complexity since the target is quite complex.
    ///     It is mainly to ensure smooth behaviour of source editor where the formatting is made for.
    ///     The member names by default belong to thing on the left side of the token.
    ///     Things on the right side contain this fact in their name.
    /// </summary>
    sealed class WhiteSpaces : DumpableObject, ISourcePartEdit, IEditPieces, LineGroup.IConfiguration
    {
        readonly Configuration Configuration;

        readonly SeparatorRequests SeparatorRequests;
        readonly int MinimalLineBreakCount;
        readonly SourcePart SourcePart;

        readonly ValueCache<WhiteSpaceView> WhiteSpaceViewCache;
        readonly string AnchorTargetPositionForDebug;

        internal WhiteSpaces
        (
            WhiteSpaceItem target
            , Configuration configuration
            , SeparatorRequests separatorRequests
            , string anchorTargetPositionForDebug
            , int minimalLineBreakCount
        )
        {
            (target != null).Assert();

            SourcePart = target.SourcePart;
            MinimalLineBreakCount = minimalLineBreakCount;
            Configuration = configuration;
            SeparatorRequests = separatorRequests;
            WhiteSpaceViewCache = new(() => new(target, this));
            AnchorTargetPositionForDebug = anchorTargetPositionForDebug;
            StopByObjectIds();
        }

        int? LineGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        int LineGroup.IConfiguration.MinimalLineBreakCount => MinimalLineBreakCount;
        SeparatorRequests LineGroup.IConfiguration.SeparatorRequests => SeparatorRequests;

        /// <summary>
        ///     Edits, i. e. pairs of old text/new text are generated to accomplish the target text.
        ///     The goal is, to change only things necessary to allow editors to work smoothly
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> IEditPieces.Get(IEditPiecesConfiguration parameter)
        {
            StopByObjectIds();
            if(!SeparatorRequests.Head &&
               !SeparatorRequests.Tail &&
               !SeparatorRequests.Inner &&
               MinimalLineBreakCount == 0 &&
               SourcePart.Length == 0)
                return new Edit[0];

            var indent = Configuration.IndentCount * parameter.Indent;
            return WhiteSpaceView
                .GetEdits(indent)
                .Select(edit => new Edit(edit.Remove, edit.Insert, AnchorTargetPositionForDebug + ":" + edit.Flag));
        }

        bool ISourcePartEdit.IsIndentTarget => true;

        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => SourcePart;

        protected override string GetNodeDump()
            => SourcePart.GetDumpAroundCurrent(10).CSharpQuote() + " " + base.GetNodeDump();

        [DisableDump]
        WhiteSpaceView WhiteSpaceView => WhiteSpaceViewCache.Value;
    }
}