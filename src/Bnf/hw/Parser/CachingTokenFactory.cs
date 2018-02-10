using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    /// <summary>
    ///     Provides a token factory that caches the items used.
    ///     It is also used internally, so you can define token factories,
    ///     that may use labourious functions in your token factory.
    /// </summary>
    public sealed class CachingTokenFactory : Dumpable, ILexerTokenFactory
    {
        readonly ValueCache<LexerItem[]> ClassesCache;
        readonly ValueCache<ITokenType> EndOfTextCache;
        readonly ValueCache<ITokenType> InvalidCharacterErrorCache;
        readonly ILexerTokenFactory Target;

        public CachingTokenFactory(ILexerTokenFactory target)
        {
            Target = target;
            EndOfTextCache = new ValueCache<ITokenType>(() => Target.EndOfText);
            InvalidCharacterErrorCache = new ValueCache<ITokenType>(() => Target.InvalidCharacterError);
            ClassesCache = new ValueCache<LexerItem[]>(() => Target.Classes);
        }

        ITokenType ILexerTokenFactory.EndOfText => EndOfTextCache.Value;
        ITokenType ILexerTokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;
        LexerItem[] ILexerTokenFactory.Classes => ClassesCache.Value;
    }
}