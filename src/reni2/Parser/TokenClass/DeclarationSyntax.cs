namespace Reni.Parser.TokenClass
{
    sealed internal class DeclarationSyntax : Syntax.SyntaxBase
    {
        private readonly DefineableToken _defineableToken;
        private readonly Syntax.SyntaxBase _definition;
        private readonly Token _token;

        internal DeclarationSyntax(DefineableToken defineableToken, Token token, Syntax.SyntaxBase definition)
        {
            _defineableToken = defineableToken;
            _token = token;
            _definition = definition;
            StopByObjectId(-876);
        }

        public DefineableToken DefineableToken { get { return _defineableToken; } }
        public Token Token { get { return _token; } }
        public Syntax.SyntaxBase Definition { get { return _definition; } }

        internal override Syntax.SyntaxBase CreateListSyntax(Token token, Syntax.SyntaxBase right)
        {
            if (right == null)
                return Syntax.Struct.Create(this, new Syntax.Void(token));
            return right.CreateListSyntaxReverse(this, token);
        }

        internal override Syntax.SyntaxBase CreateListSyntaxReverse(Syntax.SyntaxBase left, Token token)
        {
            return CreateListSyntax(this).CreateListSyntaxReverse(left, token);
        }

        internal override Syntax.SyntaxBase CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return CreateListSyntax(this).CreateListSyntaxReverse(left, token);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return DefineableToken.Name + ": " + Definition.DumpShort();
        }

        /// <summary>
        /// What to when syntax element is surrounded by parenthesis?
        /// </summary>
        /// <returns></returns>
        /// created 19.07.2007 23:20 on HAHOYER-DELL by hh
        public override Syntax.SyntaxBase SurroundedByParenthesis()
        {
            return CreateListSyntax(this);
        }

        internal protected override string FilePosition { get { return _token.FilePosition; } }
    }
}