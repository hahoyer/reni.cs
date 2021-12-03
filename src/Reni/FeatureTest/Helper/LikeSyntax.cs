using hw.DebugFormatter;
using Reni.Basics;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.FeatureTest.Helper
{
    abstract class LikeSyntax : DumpableObject
    {
        internal abstract void AssertLike(BinaryTree target);

        internal abstract void AssertLike(ValueSyntax syntax);
        public static LikeSyntax Null => new Empty();
        public LikeSyntax dump_print => Expression("dump_print");
        public static LikeSyntax Number(int i) => new Number(i);

        public static LikeSyntax Expression(LikeSyntax s1, string s2, LikeSyntax s3)
            => new Expression(s1, s2, s3);

        public static LikeSyntax Compound(LikeSyntax[] list, Declaration[] declarations, int[] converters)
            => new Struct(list, declarations, converters);


        public static LikeSyntax operator +(LikeSyntax x, LikeSyntax y) => x.Expression("+", y);
        public static LikeSyntax operator -(LikeSyntax x, LikeSyntax y) => x.Expression("-", y);
        public static LikeSyntax operator *(LikeSyntax x, LikeSyntax y) => x.Expression("*", y);
        public static LikeSyntax operator /(LikeSyntax x, LikeSyntax y) => x.Expression("/", y);

        public static Declaration Declaration(string name, int position) => new(name, position);

        public static LikeSyntax Symbol(string s) => new Expression(null, s, null);

        public LikeSyntax Expression(string s2, LikeSyntax s3) => new Expression(this, s2, s3);

        public LikeSyntax Expression(string s2) => new Expression(this, s2, null);

        public LikeSyntax Brackets(ITokenClass tokenClass = null) => new Brackets(this, tokenClass);
    }

    class Brackets : LikeSyntax
    {
        readonly LikeSyntax Target;
        readonly ITokenClass TokenClass;

        public Brackets(LikeSyntax target, ITokenClass tokenClass)
        {
            Target = target;
            TokenClass = tokenClass;
        }

        internal override void AssertLike(BinaryTree target)
        {
            (target.Left != null).Assert();
            (target.TokenClass is RightParenthesis).Assert();
            (target.Right == null).Assert();

            (target.Left.Left == null).Assert();
            (target.Left.TokenClass is LeftParenthesis).Assert();
            target.Left.TokenClass.IsBelongingTo(target.TokenClass).Assert();
            if(TokenClass != null)
                (target.TokenClass == TokenClass || target.Left.TokenClass == TokenClass).Assert();

            Target.AssertLike(target.Left.Right);
        }

        internal override void AssertLike(ValueSyntax syntax)
            => (syntax is EmptyList).Assert();
    }


    sealed class Empty : LikeSyntax
    {
        internal override void AssertLike(BinaryTree target)
        {
            (target.Left != null).Assert();
            (target.TokenClass is RightParenthesis).Assert();
            (target.Right == null).Assert();

            (target.Left.Left == null).Assert();
            (target.Left.TokenClass is LeftParenthesis).Assert();
            (target.Right == null).Assert();

            target.Left.TokenClass.IsBelongingTo(target.TokenClass).Assert();
        }

        internal override void AssertLike(ValueSyntax syntax)
            => (syntax is EmptyList).Assert();
    }

    sealed class Declaration
    {
        readonly string _name;
        readonly int _position;

        public Declaration(string name, int position)
        {
            _name = name;
            _position = position;
        }

        public void AssertContains(CompoundSyntax container)
        {
            var s = container.Find(_name, false);

            (s != null).Assert();
            (_position == s.Value).Assert();
        }
    }

    sealed class Struct : LikeSyntax
    {
        readonly int[] _converters;
        readonly Declaration[] _declarations;
        readonly LikeSyntax[] _list;

        public Struct(LikeSyntax[] list, Declaration[] declarations, int[] converters)
        {
            _list = list;
            _declarations = declarations;
            _converters = converters;
        }

        internal override void AssertLike(BinaryTree target)
            => NotImplementedMethod(target);

        internal override void AssertLike(ValueSyntax syntax)
        {
            var co = (CompoundSyntax)syntax;
            (_list.Length == co.PureStatements.Length).Assert();
            for(var i = 0; i < _list.Length; i++)
                _list[i].AssertLike(co.PureStatements[i]);

            (_declarations.Length == co.AllNames.Length).Assert();
            foreach(var declaration in _declarations)
                declaration.AssertContains(co);

            (_converters.Length == co.ConverterStatementPositions.Length).Assert();
            for(var i = 0; i < _converters.Length; i++)
                (_converters[i] == co.ConverterStatementPositions[i]).Assert();
        }
    }

    sealed class Expression : LikeSyntax
    {
        [EnableDump]
        readonly LikeSyntax _s1;

        [EnableDump]
        readonly string _s2;

        [EnableDump]
        readonly LikeSyntax _s3;

        public Expression(LikeSyntax s1, string s2, LikeSyntax s3)
        {
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }

        internal override void AssertLike(BinaryTree target)
        {
            (target.TokenClass.Id == _s2).Assert(() => $"\nTarget: {target.Dump()}\nPattern: {Dump()}");
            AssertLike(_s1, target.Left);
            AssertLike(_s3, target.Right);
        }

        internal override void AssertLike(ValueSyntax syntax)
        {
            var ex = (ExpressionSyntax)syntax;
            AssertLike(_s1, ex.Left);
            (ex.Definable?.Id == _s2).Assert();
            AssertLike(_s3, ex.Right);
        }

        static void AssertLike(LikeSyntax syntax, ValueSyntax right)
        {
            if(syntax == null)
                (right == null).Assert();
            else
                syntax.AssertLike(right);
        }

        static void AssertLike(LikeSyntax syntax, BinaryTree right)
        {
            if(syntax == null)
                (right == null).Assert();
            else
                syntax.AssertLike(right);
        }
    }

    sealed class Number : LikeSyntax
    {
        readonly long _i;

        internal Number(long i) => _i = i;

        internal override void AssertLike(BinaryTree target)
        {
            (target.Left == null).Assert();
            (target.Right == null).Assert();
            (target.TokenClass is TokenClasses.Number).Assert();
            (BitsConst.Convert(target.Token.Characters.Id).ToInt64() == _i).Assert();
        }

        internal override void AssertLike(ValueSyntax syntax)
        {
            var terminalSyntax = (TerminalSyntax)syntax;
            (terminalSyntax.Terminal is TokenClasses.Number).Assert();
            (terminalSyntax.ToNumber == _i).Assert();
        }
    }
}