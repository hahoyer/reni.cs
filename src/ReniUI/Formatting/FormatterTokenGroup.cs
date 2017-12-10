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

            var result = new List<ISourcePartEdit>();
            if(configuration.SpaceBeforeListItem)
                result.Add(SourcePartEditExtension.Space);

            if(configuration.SpaceAfterListItem)
                result.Add(SourcePartEditExtension.Space);

            var suffix = Suffix.Select(items => items.ToSourcePartEdit());
            foreach(var item in suffix)
            {
                if(isLineBreakRequired)
                    result.Add(SourcePartEditExtension.LineBreak);
                result.Add(item);
            }

            return result;
        }
    }
}