using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        sealed class CacheContainer
        {
            internal ISourcePartEdit[] Prefix;
            internal ISourcePartEdit[] Main;
            internal ISourcePartEdit[] Suffix;
        }

        internal static FormatterTokenGroup Create(Syntax syntax)
            => new FormatterTokenGroup
                (syntax.Token, syntax.RightNeighbor?.Token, syntax.RightSideSeparator() != SeparatorType.CloseSeparator);

        static ISourcePartEdit[] CreateSourcePartEdits(IToken token, bool returnMain = true)
        {
            var tokens = FormatterToken.Create(token, returnMain);
            return tokens.Select(i => i.ToSourcePartEdit()).ToArray();
        }

        readonly CacheContainer Cache = new CacheContainer();
        readonly bool IsCloseSeparatorOnRightSide;
        readonly IToken Token;
        readonly IToken RightNeighbor;

        FormatterTokenGroup(IToken token, IToken rightNeighbor, bool isCloseSeparatorOnRightSide)
        {
            Token = token;
            IsCloseSeparatorOnRightSide = isCloseSeparatorOnRightSide;
            RightNeighbor = rightNeighbor;
        }

        internal ISourcePartEdit[] Prefix
        {
            get
            {
                EnsureMainAndPrefix();
                return Cache.Prefix;
            }
        }

        internal ISourcePartEdit[] Main
        {
            get
            {
                EnsureMainAndPrefix();
                return Cache.Main;
            }
        }

        internal ISourcePartEdit[] Suffix
            => Cache.Suffix ?? (Cache.Suffix = CreateSourcePartEdits(RightNeighbor, returnMain: false));

        void EnsureMainAndPrefix()
        {
            if(Cache.Main != null && Cache.Prefix != null)
                return;
            var prefix = CreateSourcePartEdits(Token);
            var prefixLength = prefix.Length - 1;
            Cache.Prefix = prefix.Take(prefixLength).ToArray();
            Cache.Main = prefix.Skip(prefixLength).Take(count: 1).Concat(GetDistanceMarker()).ToArray();
        }

        IEnumerable<ISourcePartEdit> GetDistanceMarker()
        {
            var id = Token.Characters.Id;
            Tracer.ConditionalBreak(id == " b");
            if(IsCloseSeparatorOnRightSide)
                return new ISourcePartEdit[0];

            Tracer.ConditionalBreak(id == " a");
            return new[] {SourcePartEditExtension.EnsureSeparator};
        }
    }
}