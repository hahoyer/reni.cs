namespace hw.Scanner
{
    /// <summary>
    ///     Factory that can be used for instance in <see cref="TwoLayerScanner" /> to obtain tokens.
    /// </summary>
    public interface ILexerTokenFactory
    {
        /// <summary>
        ///     Returns the token type when reaching the end source.
        /// </summary>
        ITokenType EndOfText {get;}

        /// <summary>
        ///     Returns the token type when no class can be matched.
        /// </summary>
        ITokenType InvalidCharacterError {get;}

        /// <summary>
        ///     Returns the possible token classes, each class is a pair of a token class and a match function.
        /// </summary>
        LexerItem[] Classes {get;}
    }
}