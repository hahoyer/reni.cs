using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Graphics;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser.Services;

namespace Reni.Parser
{
    sealed class SimpleTokenFactory : TokenFactory<TokenClass, Services.Syntax>
    {
        protected override TokenClass GetError(Match.IError message) { throw new Exception("Syntax error: " + message); }
        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            return new FunctionCache<string, TokenClass>
            {
                {"{", new OpenToken(1)},
                {"(", new OpenToken(3)},
                {"}", new CloseToken(1)},
                {")", new CloseToken(3)}
            };
        }
        protected override TokenClass GetEndOfText() { return new CloseToken(0); }
        protected override TokenClass GetTokenClass(string name) { return CommonTokenClass; }
        protected override TokenClass GetNumber() { return CommonTokenClass; }
        protected override TokenClass GetText() { return CommonTokenClass; }
        static TokenClass CommonTokenClass { get { return new AnyTokenClass(); } }

        sealed class AnyTokenClass : TokenClass
        {
            protected override Services.Syntax Create(Services.Syntax left, SourcePart token, Services.Syntax right)
            {
                return Services.Syntax.CreateSyntax(left, token, right);
            }
        }

        sealed class CloseToken : TokenClass
        {
            [EnableDump]
            readonly int _level;
            public CloseToken(int level) { _level = level; }
            protected override Services.Syntax Create(Services.Syntax left, SourcePart token, Services.Syntax right)
            {
                Tracer.Assert(right == null);
                return left == null ? null : left.Match(_level, token);
            }
        }

        sealed class OpenToken : TokenClass
        {
            [EnableDump]
            readonly int _level;
            public OpenToken(int level) { _level = level; }
            protected override Services.Syntax Create(Services.Syntax left, SourcePart token, Services.Syntax right)
            {
                return new OpenSyntax(left, token, right, _level);
            }
        }

        sealed class OpenSyntax : Services.Syntax
        {
            readonly Services.Syntax _left;
            readonly Services.Syntax _right;
            [EnableDump]
            readonly int _level;
            public OpenSyntax(Services.Syntax left, SourcePart token, Services.Syntax right, int level)
                : base(token)
            {
                _left = left;
                _right = right;
                _level = level;
            }
            protected override IGraphTarget Left { get { return _left; } }
            protected override IGraphTarget Right { get { return _right; } }
            internal override Services.Syntax Match(int level, SourcePart token)
            {
                Tracer.Assert(_level == level);
                var argument = _right ?? new EmptySyntax(FirstToken.Combine(token));
                if(_left == null)
                    return argument;
                return _left.ParenthesisMatch(FirstToken, argument);
            }

            internal override SourcePart LastToken { get { return _right.LastToken; } }
        }

        sealed class EmptySyntax : Services.Syntax
        {
            public EmptySyntax(SourcePart token)
                : base(token)
            {}
        }
    }
}