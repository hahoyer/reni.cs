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
        int? LineGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        int? WhiteSpaceView.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        bool WhiteSpaceView.IConfiguration.IsSeparatorRequired => IsSeparatorRequired;

        int LineGroup.IConfiguration.MinimalLineBreakCount => MinimalLineBreakCount;
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

        IEnumerable<Edit> GetEdits(LineGroup item, IEditPiecesConfiguration parameter, bool isSeparatorRequired)
        {
            if(item.Main == null)
            {
                if(item.Spaces.Length == 1)
                {
                    var item1 = item.Spaces[0];
                    return GetEdits(item1, parameter, isSeparatorRequired);
                }

                NotImplementedMethod(item, parameter, isSeparatorRequired);
                return default;
            }

            if(item.Spaces.Any())
            {
                NotImplementedMethod(item, parameter, isSeparatorRequired);
                return default;
            }

            return Configuration.EmptyLineLimit <= 0
                ? new[] { new Edit(item.Main.SourcePart, "", "-LineBreak") }
                : new Edit[0];
        }

        IEnumerable<Edit> GetEdits(SpacesGroup item, IEditPiecesConfiguration parameter, bool isSeparatorRequired)
        {
            if(item.Tail == null)
            {
                if(isSeparatorRequired)
                {
                    if(item.Items.Length == 0)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        NotImplementedMethod(item, parameter, isSeparatorRequired);
                        return default;
                    }

                    if(item.Items.Length == 1)
                        return new Edit[0];

                    var sourcePart = item.Items[1].SourcePart.Start.Span(item.Items.Length - 1);
                    return new[] { new Edit(sourcePart, "", "-Spaces") };
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                NotImplementedMethod(item, parameter, isSeparatorRequired);
                return default;
            }

            NotImplementedMethod(item, parameter, isSeparatorRequired);
            return default;
        }
    }
}