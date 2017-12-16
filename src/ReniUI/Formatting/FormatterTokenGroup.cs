using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;


namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        internal static FormatterTokenGroup Create(Syntax syntax) => new FormatterTokenGroup(syntax);
        readonly Syntax Syntax;

        SourcePartEdit[] PrefixData;
        FormatterToken[] PrefixResultData;
        SourcePartEdit[] SuffixData;
        SourcePartEdit MainData;

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

        void EnsurePrefixResult()
        {
            if(MainData != null && PrefixData != null)
                return;
            var prefix = CreateSourcePartEdits(Syntax.Token);
            MainData = prefix.Last();
            PrefixData = prefix.Take(prefix.Length - 1).ToArray();
        }

        static SourcePartEdit[] CreateSourcePartEdits(IToken token, bool returnMain = true) 
            => FormatterToken.Create(token, returnMain).Select(i => i.ToSourcePartEdit()).ToArray();

        internal SourcePartEdit[] Suffix 
            => SuffixData ?? (SuffixData = CreateSourcePartEdits(Syntax.Right?.LeftMost.Token, false));

        internal IEnumerable<ISourcePartEdit> FormatListItem(bool isLineBreakRequired, Configuration configuration)
        {
            foreach(var edit in Prefix)
                yield return edit;

            if(configuration.SpaceBeforeListItem)
                yield return SourcePartEditExtension.Space;

            yield return Main;

            if(configuration.SpaceAfterListItem)
                yield return SourcePartEditExtension.Space;

            foreach(var item in Suffix)
            {
                if(isLineBreakRequired)
                    yield return SourcePartEditExtension.LineBreak;
                yield return item;
            }
        }

        internal IEnumerable<ISourcePartEdit> FormatFrameEnd(Configuration configuration)
        {
            foreach(var edit in Prefix)
                yield return edit;

            yield return SourcePartEditExtension.EndOfFile;
            yield return Main;
        }
    }
}