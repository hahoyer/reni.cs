using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

namespace hw.Scanner
{
    /// <summary>
    ///     Scanner interface that is used by <see cref="PrioParser{TSourcePart}" /> to split source into tokens.
    /// </summary>
    public interface IScanner
    {
        /// <summary>
        ///     Get the next group of tokens, that belongs together, like the actual token and leading or trailing whitespaces.
        /// </summary>
        /// <param name="sourcePosn">
        ///     The position in the source, where to start. The position is advanced to the end of the token
        ///     group.
        /// </param>
        /// <returns>A list of tokens that are taken from source position given.</returns>
        TokenGroup GetNextTokenGroup(SourcePosn sourcePosn);
    }

    public interface ILexerTokenType : IUniqueIdProvider {}

    public interface ITokenTypeFactory
    {
        ITokenType Get(string id);
    }

    public interface IFactoryTokenType : ILexerTokenType, ITokenTypeFactory {}

    public sealed class LexerToken : DumpableObject
    {
        static int NextObjectId;

        [DisableDump]
        internal readonly SourcePart SourcePart;

        [DisableDump]
        internal readonly ILexerTokenType Type;

        public LexerToken()
            : base(NextObjectId++) {}

        public LexerToken(SourcePart sourcePart, ILexerTokenType type)
        {
            SourcePart = sourcePart;
            Type = type;
        }

        protected override string GetNodeDump()
            => Type.Value + "(" + SourcePart.Position + "(" + SourcePart.Length + "))";
    }

    public interface ITokenType : IUniqueIdProvider {}

    public sealed class TokenGroup
    {
        public readonly SourcePart Characters;
        public readonly LexerToken[] PrefixItems;
        public readonly ITokenType Type;

        public TokenGroup(IEnumerable<LexerToken> prefixItems, SourcePart characters, ITokenType type)
        {
            PrefixItems = prefixItems.ToArray();
            Characters = characters;
            Type = type;
        }

        public SourcePart SourcePart 
            => PrefixItems.Select(i => i.SourcePart).Aggregate() + Characters;
    }
}