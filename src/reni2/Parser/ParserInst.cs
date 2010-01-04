using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    /// <summary>
    /// The parser singleton
    /// </summary>
    [Serializable]
    internal sealed class ParserInst
    {
        private readonly IScanner _scanner = new Scanner();

        /// <summary>
        /// Scans and parses source and creates the syntax tree
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IParsedSyntax Compile(Source source)
        {
            var sourcePosn = new SourcePosn(source, 0);
            var stack = new PushedSyntaxStack(sourcePosn, MainTokenFactory.Instance);
            while(true)
            {
                var result = stack.Apply(_scanner.CreateToken(sourcePosn, stack.TokenFactory));
                if (result != null)
                    return result;
            }
        }

    }

    internal interface IScanner
    {
        Token CreateToken(SourcePosn sourcePosn, ITokenFactory tokenFactory);
    }

    internal interface ITokenFactory
    {
        Token CreateToken(SourcePosn sourcePosn, int length);
    }

    internal sealed class PushedSyntaxStack
    {
        private readonly Stack<PushedSyntax> _data = new Stack<PushedSyntax>();

        public PushedSyntaxStack(SourcePosn sourcePosn, TokenFactory tokenFactory)
        {
            Push(null, sourcePosn.CreateStart(), tokenFactory);
        }

        internal TokenFactory TokenFactory { get { return _data.Peek().TokenFactory; } }

        private void Push(IParsedSyntax syntax, Token token, TokenFactory tokenFactory) { _data.Push(new PushedSyntax(syntax, token, tokenFactory)); }

        internal IParsedSyntax Apply(Token token)
        {
            IParsedSyntax result = null;
            var newTokenName = token.PrioTableName;
            while(true)
            {
                var relation = _data.Peek().Relation(newTokenName);
                if(relation != '+')
                    result = _data.Pop().CreateSyntax(result);

                if(relation != '-')
                {
                    if(token.TokenClass.IsEnd)
                        return result;
                    Push(result, token, token.NewTokenFactory ?? TokenFactory);
                    return null;
                }
            }
        }
    }
}