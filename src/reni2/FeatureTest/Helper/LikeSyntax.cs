using System;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.FeatureTest.Helper
{
    abstract class LikeSyntax : DumpableObject
    {
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

        public static Declaration Declaration(string name, int position)
            => new Declaration(name, position);

        public static LikeSyntax Symbol(string s) => new Expression(null, s, null);

        public LikeSyntax Expression(string s2, LikeSyntax s3) => new Expression(this, s2, s3);

        public LikeSyntax Expression(string s2) => new Expression(this, s2, null);

        internal abstract void AssertLike(BinaryTree target);

        [Obsolete("", true)]
        internal abstract void AssertLike(ValueSyntax syntax);

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
            Tracer.Assert(target.Left != null);
            Tracer.Assert(target.TokenClass is RightParenthesis);
            Tracer.Assert(target.Right == null);

            Tracer.Assert(target.Left.Left == null);
            Tracer.Assert(target.Left.TokenClass is LeftParenthesis);
            Tracer.Assert(target.Left.TokenClass.IsBelongingTo(target.TokenClass));
            if(TokenClass != null)
                Tracer.Assert(target.TokenClass == TokenClass || target.Left.TokenClass == TokenClass );

            Target.AssertLike(target.Left.Right);

        }

        [Obsolete("", true)]
        internal override void AssertLike(ValueSyntax syntax)
            => Tracer.Assert(syntax is EmptyList);
    }


    sealed class Empty : LikeSyntax
    {
        internal override void AssertLike(BinaryTree target)
        {
            Tracer.Assert(target.Left != null);
            Tracer.Assert(target.TokenClass is RightParenthesis);
            Tracer.Assert(target.Right == null);

            Tracer.Assert(target.Left.Left == null);
            Tracer.Assert(target.Left.TokenClass is LeftParenthesis);
            Tracer.Assert(target.Right == null);

            Tracer.Assert(target.Left.TokenClass.IsBelongingTo(target.TokenClass));
        }

        [Obsolete("", true)]
        internal override void AssertLike(ValueSyntax syntax)
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

        internal override void AssertLike(BinaryTree target) 
            => NotImplementedMethod(target);

        [Obsolete("", true)]
        internal override void AssertLike(ValueSyntax syntax)
        {
            var co = (CompoundSyntax)syntax;
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

        [Obsolete("", true)]
        static void AssertLike(LikeSyntax syntax, ValueSyntax right)
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

        internal override void AssertLike(BinaryTree target)
        {
            Tracer.Assert(target.TokenClass.Id == _s2, ()=>$"\nTarget: {target.Dump()}\nPattern: {Dump()}");
            AssertLike(_s1, target.Left);
            AssertLike(_s3, target.Right);
        }

        [Obsolete("", true)]
        internal override void AssertLike(ValueSyntax syntax)
        {
            var ex = (ExpressionSyntax)syntax;
            AssertLike(_s1, ex.Left);
            Tracer.Assert(ex.Definable?.Id == _s2);
            AssertLike(_s3, ex.Right);
        }
    }

    sealed class Number : LikeSyntax
    {
        readonly long _i;

        internal Number(long i) => _i = i;

        internal override void AssertLike(BinaryTree target)
        {
            var terminalSyntax = (TerminalSyntax)target.Syntax(null).Target;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(terminalSyntax.ToNumber == _i);
        }

        [Obsolete("", true)]
        internal override void AssertLike(ValueSyntax syntax)
        {
            var terminalSyntax = (TerminalSyntax)syntax;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(terminalSyntax.ToNumber == _i);
        }
    }
}