using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    /// <summary>
    /// Define scanner behaviour, i. e. grouping of characters at the lowest level. 
    /// Derive from this class, to define whitespaces, comments, literals and so on.
    /// </summary>
    public abstract class Lexer : DumpableObject
    {
        /// <summary>
        /// Items will contain 
        /// </summary>
        protected readonly IDictionary<IScannerTokenType, IMatch> Items = new Dictionary<IScannerTokenType, IMatch>();
        readonly Func<Match.IError, IScannerTokenType> Convert;

        public Lexer(Func<Match.IError, IScannerTokenType> convert) => Convert = convert;

        public LexerItem[] LexerItems<TScannerTokenType>(TScannerTokenType scannerTokenType)
            where TScannerTokenType : IScannerTokenType
            => Items
                .Concat(new[] {new KeyValuePair<IScannerTokenType, IMatch>(scannerTokenType, Any)})
                .Select(i => CreateLexerItem(i.Key, i.Value))
                .ToArray();

        protected abstract IMatch Any {get; }

        LexerItem CreateLexerItem(IScannerTokenType scannerTokenType, IMatch match)
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