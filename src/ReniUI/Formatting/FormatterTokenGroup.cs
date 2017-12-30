using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        internal static FormatterTokenGroup Create(Syntax syntax) => new FormatterTokenGroup(syntax);

        static SourcePartEdit[] CreateSourcePartEdits(IToken token, bool returnMain = true)
            => FormatterToken.Create(token, returnMain).Select(i => i.ToSourcePartEdit()).ToArray();

        readonly Syntax Syntax;
        ISourcePartEdit[] MainData;
        ISourcePartEdit[] PrefixData;
        ISourcePartEdit[] SuffixData;

        FormatterTokenGroup(Syntax syntax) => Syntax = syntax;

        internal ISourcePartEdit[] Prefix
        {
            get
            {
                EnsurePrefixResult();
                return PrefixData;
            }
        }

        internal ISourcePartEdit[] Main
        {
            get
            {
                EnsurePrefixResult();
                return MainData;
            }
        }

        internal ISourcePartEdit[] Suffix
            => SuffixData ?? (SuffixData = CreateSourcePartEdits(Syntax.RightNeigbor?.Token, false));

        IToken RightNeigborToken => Syntax.RightNeigbor?.Token;

        void EnsurePrefixResult()
        {
            if(MainData != null && PrefixData != null)
                return;
            var prefix = CreateSourcePartEdits(Syntax.Token);
            var prefixLength = prefix.Length - 1;
            PrefixData = prefix.Take(prefixLength).ToArray();
            MainData = prefix.Skip(prefixLength).Take(1).plus(GetDistanceMarker()).ToArray();
        }

        IEnumerable<ISourcePartEdit> GetDistanceMarker()
        {
            if(Syntax.RightSideSeparator() == SeparatorType.CloseSeparator)
                yield return SourcePartEditExtension.SpaceRequired;
        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatListItem
            (bool isLineBreakRequired, Configuration configuration)
        {
            yield return Prefix;
            yield return Main;
            yield return Prefix;

            if(isLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak.SingleToArray();
        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatFrameEnd()
        {
            yield return Prefix;
            yield return SourcePartEditExtension.EndOfFile.SingleToArray();
            yield return Main;
        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatChainItem(bool exlucdePrefix)
        {
            if(!exlucdePrefix)
                yield return Prefix;

            yield return Main;
        }
    }
}