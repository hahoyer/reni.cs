using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    /// <summary>
    ///     The parser singleton
    /// </summary>
    [Serializable]
    internal sealed class ParserInst
    {
        private readonly IScanner _scanner;
        private readonly ITokenFactory _tokenFactory;

        public ParserInst(IScanner scanner, ITokenFactory tokenFactory)
        {
            _scanner = scanner;
            _tokenFactory = tokenFactory;
        }

        /// <summary>
        ///     Scans and parses source and creates the syntax tree
        /// </summary>
        /// <param name = "source"></param>
        /// <returns></returns>
        public IParsedSyntax Compile(Source source)
        {
            var sourcePosn = new SourcePosn(source, 0);
            var stack = new Stack<PushedSyntax>();
            stack.Push(new PushedSyntax(sourcePosn, _tokenFactory));
            while(true)
            {
                var token = _scanner.CreateToken(sourcePosn, stack.Peek().TokenFactory);
                IParsedSyntax result = null;
                do
                {
                    var relation = stack.Peek().Relation(token.PrioTableName);
                    if(relation != '+')
                        result = stack.Pop().Syntax(result);

                    if(relation != '-')
                    {
                        if(token.TokenClass == _tokenFactory.RightParenthesisClass(0))
                            return token.Syntax(result, null);
                        stack.Push(new PushedSyntax(result, token, token.TokenClass.NewTokenFactory ?? stack.Peek().TokenFactory));
                        result = null;
                    }
                } while(result != null);
            }
        }
    }

    internal interface IScanner
    {
        Token CreateToken(SourcePosn sourcePosn, ITokenFactory tokenFactory);
    }
}