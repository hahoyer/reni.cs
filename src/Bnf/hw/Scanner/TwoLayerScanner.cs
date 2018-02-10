using System.Collections.Generic;
using hw.DebugFormatter;

namespace hw.Scanner
{
    /// <summary>
    ///     Language scanner, that returns groups of tokens at each call to <see cref="IScanner.GetNextTokenGroup" />.
    ///     A tokengroup in this implementation consists of a number of whitespace tokens and one actual token.
    ///     The token building is defined by a <see cref="ILexerTokenFactory" />. <br />
    ///     This is the recommented scanner implementation
    /// </summary>
    /// <remarks>
    ///     - The scanning process will try to match any of the <see cref="ILexerTokenFactory.Classes" /> in order provided.
    ///     If that fails, the next character will be returned with token type
    ///     <see cref="ILexerTokenFactory.InvalidCharacterError" />.<br />
    ///     - During scanning process, exceptions of type <see cref="TwoLayerScanner.Exception" /> will be catched
    ///     and converted into appropriate token<br />
    ///     - When the <see cref="ILexerTokenFactory.EndOfText" /> is returned the source position is set to invalid.<br />
    /// </remarks>
    public sealed class TwoLayerScanner : Dumpable, IScanner
    {
        sealed class Worker : DumpableObject
        {
            static void Advance(SourcePosn sourcePosn, int position)
            {
                var wasEnd = sourcePosn.IsEnd;
                sourcePosn.Position += position;
                if(wasEnd)
                    sourcePosn.IsValid = false;
            }

            readonly ImbeddedTokenType EndOfText;
            readonly ImbeddedTokenType InvalidCharacterError;

            readonly TwoLayerScanner Parent;
            readonly SourcePosn SourcePosn;

            internal Worker(TwoLayerScanner parent, SourcePosn sourcePosn)
            {
                Tracer.Assert(sourcePosn.IsValid);
                Parent = parent;
                SourcePosn = sourcePosn;
                EndOfText = new ImbeddedTokenType(TokenFactory.EndOfText);
                InvalidCharacterError = new ImbeddedTokenType(TokenFactory.InvalidCharacterError);
            }

            ILexerTokenFactory TokenFactory => Parent.TokenFactory;

            internal TokenGroup GetNextTokenGroup()
            {
                var prefixItems = new List<LexerToken>();

                while(true)
                {
                    var token = GetNextToken();
                    if(token.Type is IFactoryTokenType factory)
                        return new TokenGroup(prefixItems, token.SourcePart, factory.Get(token.SourcePart.Id));

                    prefixItems.Add(token);
                }
            }

            LexerToken GetNextToken()
            {
                try
                {
                    if(SourcePosn.IsEnd)
                        return CreateAndAdvance(0, EndOfText);

                    foreach(var item in TokenFactory.Classes)
                    {
                        var length = item.Match(SourcePosn);
                        Tracer.Assert(length == null || length >= 0);
                        if(length != null)
                            return CreateAndAdvance(length.Value, item.LexerTokenType);
                    }

                    Tracer.TraceBreak();
                    return CreateAndAdvance(1, InvalidCharacterError);
                }
                catch(Exception scannerException)
                {
                    var tokenType = new ImbeddedTokenType(scannerException.SyntaxError);
                    return CreateAndAdvance
                        (scannerException.SourcePosn - SourcePosn, tokenType);
                }
            }

            void Advance(int position) => Advance(SourcePosn, position);

            LexerToken CreateAndAdvance(int length, ILexerTokenType type)
            {
                var result = new LexerToken(SourcePart.Span(SourcePosn, length), type);
                Advance(length);
                return result;
            }
        }

        /// <summary>
        ///     Exception type that is requognized by <see cref="TwoLayerScanner" /> processing
        ///     and used to convert it into a correct token,
        ///     containing the <see cref="ITokenType " /> provided with exception
        ///     to identify the error on higher level.
        /// </summary>
        public sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly ITokenType SyntaxError;

            public Exception(SourcePosn sourcePosn, ITokenType syntaxError)
            {
                SourcePosn = sourcePosn;
                SyntaxError = syntaxError;
            }
        }

        readonly ILexerTokenFactory TokenFactory;
        public TwoLayerScanner(ILexerTokenFactory tokenFactory) => TokenFactory = tokenFactory;

        TokenGroup IScanner.GetNextTokenGroup(SourcePosn sourcePosn)
            => new Worker(this, sourcePosn).GetNextTokenGroup();
    }
}