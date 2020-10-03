using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.FeatureTest.Helper
{
    abstract class LikeSyntax : DumpableObject
    {
        public static LikeSyntax Number(int i) => new Number(i);

        public static LikeSyntax Expression(LikeSyntax s1, string s2, LikeSyntax s3)
            => new Expression(s1, s2, s3);

        public static LikeSyntax Compound
            (LikeSyntax[] list, Declaration[] declarations, int[] converters)
            => new Struct(list, declarations, converters);


        public static LikeSyntax operator+(LikeSyntax x, LikeSyntax y) => x.Expression("+", y);
        public static LikeSyntax operator-(LikeSyntax x, LikeSyntax y) => x.Expression("-", y);
        public static LikeSyntax operator*(LikeSyntax x, LikeSyntax y) => x.Expression("*", y);
        public static LikeSyntax operator/(LikeSyntax x, LikeSyntax y) => x.Expression("/", y);

        public static LikeSyntax Null => new Empty();

        public static Declaration Declaration(string name, int position)
            => new Declaration(name, position);

        public static LikeSyntax Symbol(string s) => new Expression(null, s, null);
        public LikeSyntax dump_print => Expression("dump_print");

        public LikeSyntax Expression(string s2, LikeSyntax s3) => new Expression(this, s2, s3);

        public LikeSyntax Expression(string s2) => new Expression(this, s2, null);

        internal abstract void AssertLike(BinaryTree binaryTree);
        internal abstract void AssertLike(Syntax syntax);
    }

    sealed class Empty : LikeSyntax
    {
        internal override void AssertLike(BinaryTree binaryTree)
            => Tracer.Assert(binaryTree.Value(null).Target is EmptyList);

        internal override void AssertLike(Syntax syntax)
            => Tracer.Assert(syntax is EmptyList);
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

            Tracer.Assert(s != null);
            Tracer.Assert(_position == s.Value);
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

        internal override void AssertLike(BinaryTree binaryTree) => AssertLike(binaryTree.Value(null).Target);

        internal override void AssertLike(Syntax syntax)
        {
            var co = (CompoundSyntax) syntax;
            Tracer.Assert(_list.Length == co.SyntaxStatements.Length);
            for(var i = 0; i < _list.Length; i++)
                _list[i].AssertLike(co.SyntaxStatements[i]);

            Tracer.Assert(_declarations.Length == co.AllNames.Length);
            foreach(var declaration in _declarations)
                declaration.AssertContains(co);

            Tracer.Assert(_converters.Length == co.ConverterStatementPositions.Length);
            for(var i = 0; i < _converters.Length; i++)
                Tracer.Assert(_converters[i] == co.ConverterStatementPositions[i]);
        }
    }

    sealed class Expression : LikeSyntax
    {
        static void AssertLike(LikeSyntax syntax, Syntax right)
        {
            if(syntax == null)
                Tracer.Assert(right == null);
            else
                syntax.AssertLike(right);
        }

        static void AssertLike(LikeSyntax syntax, BinaryTree right)
        {
            if(syntax == null)
                Tracer.Assert(right == null);
            else
                syntax.AssertLike(right);
        }

        readonly LikeSyntax _s1;
        readonly string _s2;
        readonly LikeSyntax _s3;

        public Expression(LikeSyntax s1, string s2, LikeSyntax s3)
        {
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }

        internal override void AssertLike(Reni.TokenClasses.BinaryTree binaryTree) 
            => AssertLike(binaryTree.Value(null).Target);

        internal override void AssertLike(Syntax syntax)
        {
            var ex = (ExpressionSyntax) syntax;
            AssertLike(_s1, ex.Left);
            Tracer.Assert(ex.Definable?.Id == _s2);
            AssertLike(_s3, ex.Right);
        }
    }

    sealed class Number : LikeSyntax
    {
        readonly long _i;

        internal Number(long i) => _i = i;

        internal override void AssertLike(BinaryTree binaryTree)
        {
            var terminalSyntax = (TerminalSyntax) binaryTree.Value(null).Target;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(terminalSyntax.ToNumber == _i);
        }

        internal override void AssertLike(Syntax syntax)
        {
            var terminalSyntax = (TerminalSyntax) syntax;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(terminalSyntax.ToNumber == _i);
        }
    }
}