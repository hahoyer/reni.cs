using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    /// <summary>
    ///     Incomplete syntax tree element
    /// </summary>
    internal sealed class PushedSyntax : ReniObject
    {
        private readonly IParsedSyntax _left;
        private readonly Token _token;
        private readonly ITokenFactory _tokenFactory;

        internal PushedSyntax(IParsedSyntax left, Token token, ITokenFactory tokenFactory)
        {
            _left = left;
            _token = token;
            _tokenFactory = tokenFactory;
        }

        internal PushedSyntax(SourcePosn sourcePosn, ITokenFactory tokenFactory)
            : this(null, new Token(sourcePosn, 0, tokenFactory.LeftParenthesisClass(0)), tokenFactory) { }

        internal ITokenFactory TokenFactory { get { return _tokenFactory; } }

        internal char Relation(string newTokenName) { return _tokenFactory.PrioTable.Relation(newTokenName, _token.PrioTableName); }

        public IParsedSyntax Syntax(IParsedSyntax args) { return _token.Syntax(_left, args); }

        public override string ToString()
        {
            if(_left == null)
                return "null " + _token.PrioTableName;
            return _left.DumpShort() + " " + _token.PrioTableName + " " + _tokenFactory;
        }
    }
}