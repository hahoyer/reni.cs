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
    sealed class WhiteSpaces : DumpableObject, ISourcePartEdit, IEditPieces, WhiteSpaceView.IConfiguration
    {
        readonly Configuration Configuration;

        readonly bool IsSeparatorRequired;
        readonly int MinimalLineBreakCount;
        readonly SourcePart SourcePart;
        readonly bool HasLines;

        readonly ValueCache<WhiteSpaceView> WhiteSpaceViewCache;

        internal WhiteSpaces
        (
            WhiteSpaceItem target
            , Configuration configuration
            , bool isSeparatorRequired
            , int minimalLineBreakCount = 0
        )
        {
            (target != null).Assert();
            SourcePart = target.SourcePart;
            HasLines = target.TargetLineBreakCount > 0;
            IsSeparatorRequired = isSeparatorRequired;
            MinimalLineBreakCount = minimalLineBreakCount;
            Configuration = configuration;
            WhiteSpaceViewCache = new(() => new(target, this));
            StopByObjectIds();
        }

        int? CommentGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        int? WhiteSpaceView.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        bool WhiteSpaceView.IConfiguration.IsSeparatorRequired => IsSeparatorRequired;

        int CommentGroup.IConfiguration.MinimalLineBreakCount => MinimalLineBreakCount;
        int WhiteSpaceView.IConfiguration.MinimalLineBreakCount => MinimalLineBreakCount;

        /// <summary>
        ///     Edits, i. e. pairs of old text/new text are generated to accomplish the target text.
        ///     The goal is, to change only things necessary to allow editors to work smoothly
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> IEditPieces.Get(IEditPiecesConfiguration parameter)
        {
            if(!IsSeparatorRequired && MinimalLineBreakCount == 0 && SourcePart.Length == 0)
                return new Edit[0];

            return WhiteSpaceView
                .GetEdits(parameter.Indent)
                .Select(edit => new Edit(edit.Location, edit.NewText, edit.Flag));
        }

        bool ISourcePartEdit.HasLines => HasLines;

        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => SourcePart;

        protected override string GetNodeDump()
            => SourcePart.GetDumpAroundCurrent(10).CSharpQuote() + " " + base.GetNodeDump();

        [DisableDump]
        WhiteSpaceView WhiteSpaceView => WhiteSpaceViewCache.Value;
    }
}