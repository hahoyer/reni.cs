using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Forms;
using hw.Graphics;
using hw.Helper;
using hw.Parser;
using hw.PrioParser;

namespace Reni.Parser
{
    sealed class SimpleTokenFactory : TokenFactory<SimpleTokenFactory.TokenClass>
    {
        public SimpleTokenFactory(PrioTable prioTable)
            : base(prioTable) { }

        protected override TokenClass GetSyntaxError(string message) { throw new Exception("Syntax error: " + message); }
        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            return new FunctionCache<string, TokenClass>
            {
                {"{", new OpenToken(1)},
                {"(", new OpenToken(3)},
                {"}", new CloseToken(1)},
                {")", new CloseToken(3)},
            };
        }
        protected override TokenClass GetEndOfText() { return new CloseToken(0); }
        protected override TokenClass GetBeginOfText() { return new OpenToken(0); }
        protected override TokenClass GetTokenClass(string name) { return CommonTokenClass; }
        protected override TokenClass GetNumber() { return CommonTokenClass; }
        protected override TokenClass GetText() { return CommonTokenClass; }
        static TokenClass CommonTokenClass { get { return new AnyTokenClass(); } }

        internal abstract class Syntax : DumpableObject, IParsedSyntax, IGraphTarget
        {
            [EnableDump]
            readonly TokenData _token;

            protected Syntax(TokenData token) { _token = token; }

            [DisableDump]
            string IIconKeyProvider.IconKey { get { return "Syntax"; } }

            [DisableDump]
            TokenData IParsedSyntax.Token { get { return _token; } }

            [DisableDump]
            TokenData IParsedSyntax.FirstToken { get { return FirstToken; } }

            [DisableDump]
            TokenData IParsedSyntax.LastToken { get { return LastToken; } }

            [DisableDump]
            protected virtual TokenData FirstToken { get { return _token; } }

            [DisableDump]
            internal virtual TokenData LastToken { get { return _token; } }

            public string Title { get { return _token.Name; } }

            [DisableDump]
            public IGraphTarget[] Children { get { return new[] {Left, Right}; } }

            protected virtual IGraphTarget Right { get { return null; } }
            protected virtual IGraphTarget Left { get { return null; } }

            string IParsedSyntax.Dump() { return Dump(); }
            string IParsedSyntax.GetNodeDump() { return NodeDump; }

            internal static Syntax CreateSyntax(Syntax left, TokenData token, Syntax right)
            {
                if(left == null)
                {
                    if(right == null)
                        return new TerminalSyntax(token);
                    return new PrefixSyntax(token, right);
                }

                if(right == null)
                    return new SuffixSyntax(left, token);
                return new InfixSyntax(left, token, right);
            }

            sealed class InfixSyntax : Syntax
            {
                readonly Syntax _left;
                readonly Syntax _right;
                public InfixSyntax(Syntax left, TokenData token, Syntax right)
                    : base(token)
                {
                    _left = left;
                    _right = right;
                }
                protected override TokenData FirstToken { get { return _left.FirstToken; } }
                internal override TokenData LastToken { get { return _right.LastToken; } }
                protected override IGraphTarget Right { get { return _right; } }
                protected override IGraphTarget Left { get { return _left; } }
            }

            sealed class SuffixSyntax : Syntax
            {
                readonly Syntax _left;
                public SuffixSyntax(Syntax left, TokenData token)
                    : base(token) { _left = left; }
                protected override TokenData FirstToken { get { return _left.FirstToken; } }
                protected override IGraphTarget Left { get { return _left; } }
            }

            sealed class PrefixSyntax : Syntax
            {
                readonly Syntax _right;
                public PrefixSyntax(TokenData token, Syntax right)
                    : base(token) { _right = right; }
                internal override TokenData LastToken { get { return _right.LastToken; } }
                protected override IGraphTarget Right { get { return _right; } }
            }

            sealed class TerminalSyntax : Syntax
            {
                public TerminalSyntax(TokenData token)
                    : base(token) { }
                internal override Syntax ParenthesisMatch(TokenData token, Syntax argument) { return CreateSyntax(null, FirstToken, argument); }
            }

            internal virtual Syntax Match(int level, TokenData token) { return new InfixSyntax(this, token, null); }
            internal virtual Syntax ParenthesisMatch(TokenData token, Syntax argument) { return CreateSyntax(this, token, argument); }
        }

        internal new abstract class TokenClass : DumpableObject, IType<IParsedSyntax>, INameProvider
        {
            string _name;
            IParsedSyntax IType<IParsedSyntax>.Create(IParsedSyntax left, IPart part, IParsedSyntax right, bool isMatch)
            {
                return CreateSyntax((Syntax)left, (TokenData)part, (Syntax)right);
            }
            string IType<IParsedSyntax>.PrioTableName { get { return _name; } }
            bool IType<IParsedSyntax>.IsEnd { get { return _name == PrioTable.EndOfText; } }
            protected abstract Syntax CreateSyntax(Syntax left, TokenData token, Syntax right);
            string INameProvider.Name { set { _name = value; } }
        }

        sealed class AnyTokenClass : TokenClass
        {
            protected override Syntax CreateSyntax(Syntax left, TokenData token, Syntax right) { return Syntax.CreateSyntax(left, token, right); }
        }

        sealed class CloseToken : TokenClass
        {
            [EnableDump]
            readonly int _level;
            public CloseToken(int level) { _level = level; }
            protected override Syntax CreateSyntax(Syntax left, TokenData token, Syntax right)
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
            protected override Syntax CreateSyntax(Syntax left, TokenData token, Syntax right) { return new OpenSyntax(left, token, right, _level); }
        }

        sealed class OpenSyntax : Syntax
        {
            readonly Syntax _left;
            readonly Syntax _right;
            [EnableDump]
            readonly int _level;
            public OpenSyntax(Syntax left, TokenData token, Syntax right, int level)
                : base(token)
            {
                _left = left;
                _right = right;
                _level = level;
            }
            protected override IGraphTarget Left { get { return _left; } }
            protected override IGraphTarget Right { get { return _right; } }
            internal override Syntax Match(int level, TokenData token)
            {
                Tracer.Assert(_level == level);
                var argument = _right ?? new EmptySyntax(FirstToken.Combine(token));
                if(_left == null)
                    return argument;
                return _left.ParenthesisMatch(FirstToken, argument);
            }

            internal override TokenData LastToken { get { return _right.LastToken; } }
        }

        sealed class EmptySyntax : Syntax
        {
            public EmptySyntax(TokenData token)
                : base(token) { }
        }
    }
}