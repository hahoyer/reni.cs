using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        internal static FormatterTokenGroup Create(Syntax syntax) => new FormatterTokenGroup(syntax);

        static ISourcePartEdit[] CreateSourcePartEdits(IToken token, bool returnMain = true)
        {
            var tokens = FormatterToken.Create(token, returnMain);
            return tokens.Select(i => i.ToSourcePartEdit()).ToArray();
        }

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

        void EnsurePrefixResult()
        {
            if(MainData != null && PrefixData != null)
                return;
            var prefix = CreateSourcePartEdits(Syntax.Token);
            var prefixLength = prefix.Length - 1;
            PrefixData = prefix.Take(prefixLength).ToArray();
            MainData = prefix.Skip(prefixLength).Take(1).Concat(GetDistanceMarker()).ToArray();
        }

        IEnumerable<ISourcePartEdit> GetDistanceMarker()
        {
            var id = Syntax.Token.Characters.Id;
            Tracer.ConditionalBreak(id == " b");
            if(Syntax.RightSideSeparator() != SeparatorType.CloseSeparator)
                return new ISourcePartEdit[0];
            
            Tracer.ConditionalBreak(id == " a");
            return new [] {SourcePartEditExtension.EnsureSeparator};

        }

        internal IEnumerable<IEnumerable<ISourcePartEdit>> FormatChainItem(bool exlucdePrefix)
        {
            if(!exlucdePrefix)
                yield return Prefix;

            yield return Main;
        }
    }
}