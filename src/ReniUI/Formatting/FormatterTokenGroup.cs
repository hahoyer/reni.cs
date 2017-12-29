using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        internal static FormatterTokenGroup Create(Syntax syntax) => new FormatterTokenGroup(syntax);

        static SourcePartEdit[] CreateSourcePartEdits(IToken token, bool returnMain = true)
            => FormatterToken.Create(token, returnMain).Select(i => i.ToSourcePartEdit()).ToArray();

        readonly Syntax Syntax;
        SourcePartEdit MainData;

        SourcePartEdit[] PrefixData;
        FormatterToken[] PrefixResultData;
        SourcePartEdit[] SuffixData;

        FormatterTokenGroup(Syntax syntax) => Syntax = syntax;

        internal SourcePartEdit[] Prefix
        {
            get
            {
                EnsurePrefixResult();
                return PrefixData;
            }
        }

        internal SourcePartEdit Main
        {
            get
            {
                EnsurePrefixResult();
                return MainData;
            }
        }

        internal SourcePartEdit[] Suffix
            => SuffixData ?? (SuffixData = CreateSourcePartEdits(Syntax.RightNeigbor?.Token, false));

        void EnsurePrefixResult()
        {
            if(MainData != null && PrefixData != null)
                return;
            var prefix = CreateSourcePartEdits(Syntax.Token);
            MainData = prefix.Last();
            PrefixData = prefix.Take(prefix.Length - 1).ToArray();
        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatListItem
            (bool isLineBreakRequired, Configuration configuration)
        {
            yield return Prefix;

            if(configuration.SpaceBeforeListItem)
                yield return SourcePartEditExtension.Space.SingleToArray();

            yield return Main.SingleToArray();

            if(configuration.SpaceAfterListItem)
                yield return SourcePartEditExtension.Space.SingleToArray();

            foreach(var item in Suffix)
            {
                if(isLineBreakRequired)
                    yield return SourcePartEditExtension.LineBreak.SingleToArray();
                yield return item.SingleToArray();
            }

            if(isLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak.SingleToArray();
        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatFrameEnd()
        {
            yield return Prefix;
            yield return SourcePartEditExtension.EndOfFile.SingleToArray();
            yield return Main.SingleToArray();
        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatChainItem(bool exlucdePrefix)
        {
            if(!exlucdePrefix)
                yield return Prefix;

            yield return Main.SingleToArray();
        }
    }
}