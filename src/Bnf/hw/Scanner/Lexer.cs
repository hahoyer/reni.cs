using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    /// <summary>
    ///     Define scanner behaviour, i. e. grouping of characters at the lowest level.
    ///     Derive from this class, to define whitespaces, comments, literals and so on.
    /// </summary>
    public abstract class Lexer : DumpableObject
    {
        /// <summary>
        ///     Items will contain
        /// </summary>
        protected readonly IDictionary<ILexerTokenType, IMatch> Items = new Dictionary<ILexerTokenType, IMatch>();

        readonly Func<Match.IError, ITokenType> Convert;

        public Lexer(Func<Match.IError, ITokenType> convert) => Convert = convert;

        protected abstract IMatch Any {get;}

        public LexerItem[] LexerItems(ITokenTypeFactory tokenTypeForAny)
            => Items
                .Concat(CreateItemForAny(tokenTypeForAny))
                .Select(i => CreateLexerItem(i.Key, i.Value))
                .ToArray();


        IEnumerable<KeyValuePair<ILexerTokenType, IMatch>> CreateItemForAny(ITokenTypeFactory factory)
        {
            var tokenTypeForAny = new ImbeddedTokenFactoryType(factory, "<any>");
            yield return new KeyValuePair<ILexerTokenType, IMatch>(tokenTypeForAny, Any);
        }

        LexerItem CreateLexerItem(ILexerTokenType scannerTokenType, IMatch match)
            => new LexerItem(scannerTokenType, sourcePosn => GuardedMatch(sourcePosn, match));

        public int? GuardedMatch(SourcePosn sourcePosn, IMatch match)
        {
            try
            {
                return sourcePosn.Match(match);
            }
            catch(Match.Exception exception)
            {
                throw new TwoLayerScanner.Exception
                (
                    exception.SourcePosn,
                    Convert(exception.Error)
                );
            }
        }
    }
}