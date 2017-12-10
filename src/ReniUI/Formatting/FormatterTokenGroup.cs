using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        internal static FormatterTokenGroup Create(Syntax syntax) => new FormatterTokenGroup(syntax);
        readonly Syntax Syntax;

        FormatterToken[] PrefixData;
        FormatterToken[] PrefixResultData;
        FormatterToken[] SuffixData;

        FormatterTokenGroup(Syntax syntax) => Syntax = syntax;

        internal FormatterToken[] Prefix
            => PrefixData ?? (PrefixData = PrefixResult.ToArray());

        internal FormatterToken[] Suffix
            => SuffixData ?? (SuffixData = FormatterToken.CreateOther(Syntax.Right?.LeftMost.Token).ToArray());

        FormatterToken[] PrefixResult
            => PrefixResultData ?? (PrefixResultData = FormatterToken.Create(Syntax.Token).ToArray());

        internal IEnumerable<ISourcePartEdit> FormatListItem(bool isLineBreakRequired, Configuration configuration)
        {
            Tracer.Assert(!Prefix.Any());

            if(configuration.SpaceBeforeListItem)
                yield return SourcePartEditExtension.Space;

            if(configuration.SpaceAfterListItem)
                yield return SourcePartEditExtension.Space;

            var suffix = Suffix.Select(items => items.ToSourcePartEdit());
            foreach(var item in suffix)
            {
                if(isLineBreakRequired)
                    yield return SourcePartEditExtension.LineBreak;
                yield return item;
            }
        }

        internal IEnumerable<ISourcePartEdit> FormatFrameEnd(Configuration configuration)
        {
            for(var i = 0; i < Prefix.Length; i++)
            {
                if(i >= Prefix.Length - 1)
                    yield return SourcePartEditExtension.EndOfFile;
                yield return Prefix[i].ToSourcePartEdit();
            }
        }
    }
}