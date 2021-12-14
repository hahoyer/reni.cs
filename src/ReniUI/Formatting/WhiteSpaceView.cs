using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    /// <summary>
    ///     Encapsulates all comment, line-break and space formatting for a token.
    ///     This class has a very high complexity since the target is quite complex.
    ///     It is mainly to ensure smooth behaviour of source editor where the formatting is made for.
    ///     The member names by default belong to thing on the left side of the token.
    ///     Things on the right side contain this fact in their name.
    /// </summary>
    sealed class WhiteSpaceView : DumpableObject, ISourcePartEdit, IEditPieces, StableLineGroup.IConfiguration
    {
        readonly Configuration Configuration;

        readonly bool IsSeparatorRequired;
        readonly int MinimalLineBreakCount;

        readonly WhitespaceItem[] FlatItems;
        readonly SourcePart SourcePart;
        readonly bool HasLines;

        StableLineGroup[] StableLineGroupsCache;

        internal WhiteSpaceView
        (
            WhitespaceItem target
            , Configuration configuration
            , bool isSeparatorRequired
            , int minimalLineBreakCount = 0
        )
        {
            (target != null).Assert();
            FlatItems = GetFlatItems(target).ToArray();
            SourcePart = target.SourcePart;
            HasLines = target.TargetLineBreakCount > 0;
            IsSeparatorRequired = isSeparatorRequired;
            MinimalLineBreakCount = minimalLineBreakCount;
            Configuration = configuration;
            StopByObjectIds();
        }

        int? StableLineGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;

        int StableLineGroup.IConfiguration.MinimalLineBreakCount => MinimalLineBreakCount;

        /// <summary>
        ///     Edits, i. e. pairs of old text/new text are generated to accomplish the target text.
        ///     The goal is, to change only things necessary to allow editors to work smoothly
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> IEditPieces.Get(IEditPiecesConfiguration parameter)
        {
            if(!IsSeparatorRequired && MinimalLineBreakCount == 0 && SourcePart.Length == 0)
                return new Edit[0];

            return StableLineGroups.SelectMany((item, index) => item.Get(parameter, GetIsSeparatorRequired(index)));
        }

        bool ISourcePartEdit.HasLines => HasLines;

        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => SourcePart;

        protected override string GetNodeDump()
            => SourcePart.GetDumpAroundCurrent(10).CSharpQuote() + " " + base.GetNodeDump();

        [DisableDump]
        StableLineGroup[] StableLineGroups
            => StableLineGroupsCache ??= StableLineGroup.Create(FlatItems, this).ToArray();

        bool GetIsSeparatorRequired(int index)
        {
            if(index == 0)
                return IsSeparatorRequired;
            return StableLineGroups[index - 1].IsSeparatorRequired;
        }

        static IEnumerable<WhitespaceItem> GetFlatItems(WhitespaceItem target)
            => target.Items.Any()
                ? target.Items.SelectMany(GetFlatItems)
                : new[] { target };
    }
}