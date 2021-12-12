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
        public LikeSyntax DumpPrint => Expression("dump_print");
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
        readonly string Name;
        readonly int Position;

        public Declaration(string name, int position)
        {
            Name = name;
            Position = position;
        }

        public void AssertContains(CompoundSyntax container)
        {
            var s = container.Find(Name, false);

            (s != null).Assert();
            (Position == s.Value).Assert();
        }
    }

    sealed class Struct : LikeSyntax
    {
        readonly int[] Converters;
        readonly Declaration[] Declarations;
        readonly LikeSyntax[] List;

        public Struct(LikeSyntax[] list, Declaration[] declarations, int[] converters)
        {
            List = list;
            Declarations = declarations;
            Converters = converters;
        }

        internal override void AssertLike(BinaryTree target)
            => NotImplementedMethod(target);

        internal override void AssertLike(ValueSyntax syntax)
        {
            var co = (CompoundSyntax)syntax;
            (List.Length == co.PureStatements.Length).Assert();
            for(var i = 0; i < List.Length; i++)
                List[i].AssertLike(co.PureStatements[i]);

            (Declarations.Length == co.AllNames.Length).Assert();
            foreach(var declaration in Declarations)
                declaration.AssertContains(co);

            (Converters.Length == co.ConverterStatementPositions.Length).Assert();
            for(var i = 0; i < Converters.Length; i++)
                (Converters[i] == co.ConverterStatementPositions[i]).Assert();
        }
    }

    sealed class Expression : LikeSyntax
    {
        [EnableDump]
        readonly LikeSyntax S1;

        [EnableDump]
        readonly string S2;

        [EnableDump]
        readonly LikeSyntax S3;

        public Expression(LikeSyntax s1, string s2, LikeSyntax s3)
        {
            S1 = s1;
            S2 = s2;
            S3 = s3;
        }

        internal override void AssertLike(BinaryTree target)
        {
            (target.TokenClass.Id == S2).Assert(() => $"\nTarget: {target.Dump()}\nPattern: {Dump()}");
            AssertLike(S1, target.Left);
            AssertLike(S3, target.Right);
        }

        internal override void AssertLike(ValueSyntax syntax)
        {
            var ex = (ExpressionSyntax)syntax;
            AssertLike(S1, ex.Left);
            (ex.Definable?.Id == S2).Assert();
            AssertLike(S3, ex.Right);
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
        readonly long I;

        internal Number(long i) => I = i;

        internal override void AssertLike(BinaryTree target)
        {
            (target.Left == null).Assert();
            (target.Right == null).Assert();
            (target.TokenClass is TokenClasses.Number).Assert();
            (BitsConst.Convert(target.Token.Id).ToInt64() == I).Assert();
        }

        internal override void AssertLike(ValueSyntax syntax)
        {
            var terminalSyntax = (TerminalSyntax)syntax;
            (terminalSyntax.Terminal is TokenClasses.Number).Assert();
            (terminalSyntax.ToNumber == I).Assert();
        }
    }
}