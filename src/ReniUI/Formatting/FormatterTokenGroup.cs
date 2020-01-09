using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterTokenGroup : DumpableObject
    {
        sealed class CacheContainer
        {
            internal ISourcePartEdit[] TokenEdits;
            internal ISourcePartEdit[] PrefixEdits;
            internal ISourcePartEdit[] SuffixEdits;
        }

        internal static FormatterTokenGroup Create(Syntax syntax)
            => new FormatterTokenGroup
            (
                syntax.Token,
                syntax.RightNeighbor?.Token,
                syntax.RightSideSeparator() != SeparatorType.CloseSeparator);

        static ISourcePartEdit[] CreateSourcePartEdits(IToken token, bool returnMain = true)
        {
            var tokens = FormatterToken.Create(token, returnMain);
            return tokens.Select(i => i.ToSourcePartEdit()).ToArray();
        }

        readonly CacheContainer Cache = new CacheContainer();
        readonly IToken Token;
        readonly IToken RightNeighbor;
        readonly bool IsCloseSeparatorOnRightSide;

        FormatterTokenGroup(IToken token, IToken rightNeighbor, bool isCloseSeparatorOnRightSide)
        {
            Token = token;
            IsCloseSeparatorOnRightSide = isCloseSeparatorOnRightSide;
            RightNeighbor = rightNeighbor;
        }

        /// <summary>
        /// Returns all edits, except the last. So it's all comments and whitespaces before the actual token
        /// </summary>
        internal ISourcePartEdit[] PrefixEdits
        {
            get
            {
                EnsureMainAndPrefix();
                return Cache.PrefixEdits;
            }
        }

        /// <summary>
        /// Returns the last edits, except the last. So this is the actual token
        /// </summary>
        internal ISourcePartEdit[] TokenEdits
        {
            get
            {
                EnsureMainAndPrefix();
                return Cache.TokenEdits;
            }
        }

        /// <summary>
        /// Returns the prefix edits of the right neighbor token
        /// </summary>
        internal ISourcePartEdit[] SuffixEdits
            => Cache.SuffixEdits ?? (Cache.SuffixEdits = CreateSourcePartEdits(RightNeighbor, returnMain: false));

        void EnsureMainAndPrefix()
        {
            if(Cache.TokenEdits != null && Cache.PrefixEdits != null)
                return;
            var prefix = CreateSourcePartEdits(Token);
            var prefixLength = prefix.Length - 1;
            Cache.PrefixEdits = prefix.Take(prefixLength).ToArray();
            Cache.TokenEdits = prefix.Skip(prefixLength).Take(count: 1).Concat(GetDistanceMarker()).ToArray();
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

        protected override string GetNodeDump() => $"{Token.Characters.Id} {RightNeighbor.Characters.Id}";
    }
}