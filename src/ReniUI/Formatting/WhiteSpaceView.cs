using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;
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
    sealed class WhiteSpaceView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        sealed class CacheContainer { }

        readonly CacheContainer Cache = new();
        readonly Configuration Configuration;

        readonly bool IsSeparatorRequired;
        readonly int MinimalLineBreakCount;

        readonly WhitespaceGroup Target;

        internal WhiteSpaceView
        (
            WhitespaceGroup target
            , Configuration configuration
            , bool isSeparatorRequired
            , int minimalLineBreakCount = 0
        )
        {
            (target != null).Assert();
            Target = target;
            IsSeparatorRequired = isSeparatorRequired;
            MinimalLineBreakCount = minimalLineBreakCount;
            Configuration = configuration;
            StopByObjectIds();
        }

        /// <summary>
        ///     Edits, i. e. pairs of old text/new text are generated to accomplish the target text.
        ///     The goal is, to change only things necessary to allow editors to work smoothly
        /// </summary>
        /// <returns></returns>
        [Obsolete("", true)]
        IEnumerable<Edit> IEditPieces.Get(IEditPiecesConfiguration parameter)
        {
            yield break;
            NotImplementedMethod(parameter);
        }

        bool ISourcePartEdit.HasLines => Target.TargetLineBreakCount > 0;

        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => Target.SourcePart;

        protected override string GetNodeDump()
            => Target.SourcePart.GetDumpAroundCurrent(10).CSharpQuote() + " " + base.GetNodeDump();


        [Obsolete("", true)]
        static SourcePart GetLineBreakPart(IEnumerable<IItem> group)
        {
            var last = group.Last();
            last.HasLines().Assert();
            if(last.IsLineEnd())
                return group.SourcePart();
            last.IsLineComment().Assert();
            return default;
            //return last.GetLineBreakAtEnd();
        }

    }
}