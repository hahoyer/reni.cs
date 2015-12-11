using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Graphics;
using hw.Parser;
using hw.Scanner;

namespace Reni.ParserTest
{
    sealed class SimpleTokenFactory : TokenFactory<Services.TokenClass, Services.Syntax>
    {
        protected override Services.TokenClass GetError(Match.IError message)
        {
            throw new Exception("Syntax error: " + message);
        }

        protected override IEnumerable<Services.TokenClass> GetPredefinedTokenClasses()
            => new Services.TokenClass[]
            {
                new OpenToken("{", 1),
                new OpenToken("(", 3),
                new CloseToken("}", 1),
                new CloseToken(")", 3)
            };

        protected override Services.TokenClass GetEndOfText()
            => new CloseToken(PrioTable.EndOfText, 0);

        protected override Services.TokenClass GetTokenClass(string name) => new AnyTokenClass(name);
        protected override Services.TokenClass GetNumber() => CommonTokenClass;
        protected override Services.TokenClass GetText() => CommonTokenClass;
        static Services.TokenClass CommonTokenClass => new AnyTokenClass();

        sealed class AnyTokenClass : Services.TokenClass
        {
            public AnyTokenClass()
                : base(PrioTable.Any) {}

            public AnyTokenClass(string id)
                : base(id) {}
        }

        sealed class CloseToken : Services.TokenClass
        {
            [EnableDump]
            readonly int _level;

            public CloseToken(string id, int level)
                : base(id)
            {
                _level = level;
            }
        }

        sealed class OpenToken : Services.TokenClass
        {
            [EnableDump]
            readonly int _level;

            public OpenToken(string id, int level)
                : base(id)
            {
                _level = level;
            }
        }

        sealed class OpenSyntax : Services.Syntax
        {
            readonly Services.Syntax _left;
            readonly Services.Syntax _right;
            [EnableDump]
            readonly int _level;

            public OpenSyntax
                (Services.Syntax left, IToken token, Services.Syntax right, int level)
                : base(token)
            {
                _left = left;
                _right = right;
                _level = level;
            }

            protected override IGraphTarget Left => _left;
            protected override IGraphTarget Right => _right;

            internal override Services.Syntax Match(int level, IToken token)
            {
                Tracer.Assert(_level == level);
                var argument = _right ?? new EmptySyntax(token);
                if(_left == null)
                    return argument;
                return _left.ParenthesisMatch(Token, argument);
            }
        }

        sealed class EmptySyntax : Services.Syntax
        {
            public EmptySyntax(IToken token)
                : base(token) {}
        }
    }
}