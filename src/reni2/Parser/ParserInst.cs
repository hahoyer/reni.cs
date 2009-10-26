using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    /// <summary>
    /// The parser singleton
    /// </summary>
    [Serializable]
    internal sealed class ParserInst
    {
        private readonly Scanner _scanner = new Scanner();

        /// <summary>
        /// Scans and parses source and creates the syntax tree
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IParsedSyntax Compile(Source source)
        {
            var sourcePosn = new SourcePosn(source, 0);
            IParsedSyntax start = null;
            var stack = new PushedSyntaxStack(sourcePosn, new TokenFactory<TokenAttribute>());
            while(stack.Apply(ref start, _scanner.CreateToken(sourcePosn, stack.TokenFactory)))
                start = null;
            return start;
        }

    }

    internal sealed class PushedSyntaxStack
    {
        private readonly Stack<PushedSyntax> _data = new Stack<PushedSyntax>();

        public PushedSyntaxStack(SourcePosn sourcePosn, TokenFactory tokenFactory)
        {
            _data.Push(new PushedSyntax(null, sourcePosn.CreateStart(), tokenFactory));
        }

        internal TokenFactory TokenFactory { get { return _data.Peek().TokenFactory; } }

        internal bool Apply(ref IParsedSyntax syntax, Token token)
        {
            while(true)
            {
                var relation = _data.Peek().Relation(token);
                if(relation != '+')
                    syntax = PullAndCall(syntax);

                if(relation != '-')
                {
                    if (token.TokenClass.IsEnd)
                        return false;
                    var tokenFactory = token.NewTokenFactory ?? _data.Peek().TokenFactory;
                    _data.Push(new PushedSyntax(syntax, token, tokenFactory));
                    return true;
                }
            }
        }

        private IParsedSyntax PullAndCall(IParsedSyntax args)
        {
            var x = _data.Pop();
            return x.CreateSyntax(args);
        }

    }
}