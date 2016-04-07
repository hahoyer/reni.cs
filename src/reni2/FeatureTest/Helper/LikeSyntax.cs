using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.FeatureTest.Helper
{
    abstract class LikeSyntax : DumpableObject
    {
        public static LikeSyntax Number(int i) => new Number(i);

        public LikeSyntax Expression(string s2, LikeSyntax s3) => new Expression(this, s2, s3);

        public static LikeSyntax Expression(LikeSyntax s1, string s2, LikeSyntax s3)
            => new Expression(s1, s2, s3);

        public LikeSyntax Expression(string s2) => new Expression(this, s2, null);

        public static LikeSyntax Compound
            (LikeSyntax[] list, Declaration[] declarations, int[] converters)
            => new Struct(list, declarations, converters);

        public abstract void AssertLike(Syntax syntax);

        public static LikeSyntax operator +(LikeSyntax x, LikeSyntax y) => x.Expression("+", y);
        public static LikeSyntax operator -(LikeSyntax x, LikeSyntax y) => x.Expression("-", y);
        public static LikeSyntax operator *(LikeSyntax x, LikeSyntax y) => x.Expression("*", y);
        public static LikeSyntax operator /(LikeSyntax x, LikeSyntax y) => x.Expression("/", y);
        public LikeSyntax dump_print => Expression("dump_print");

        public static LikeSyntax Null => new Empty();

        public static Declaration Declaration(string name, int position)
            => new Declaration(name, position);

        public static LikeSyntax Symbol(string s) => new Expression(null, s, null);

        public void AssertLike(Value syntax) => NotImplementedMethod(syntax);
    }

    sealed class Empty : LikeSyntax
    {
        public override void AssertLike(Syntax syntax)
            => Tracer.Assert(syntax.Value.Value is EmptyList);
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
            var s = container.Find(_name);

            Tracer.Assert(s != null);
            Tracer.Assert(_position == s.Value);
        }
    }

    sealed class Struct : LikeSyntax
    {
        readonly LikeSyntax[] _list;
        readonly Declaration[] _declarations;
        readonly int[] _converters;

        public Struct(LikeSyntax[] list, Declaration[] declarations, int[] converters)
        {
            _list = list;
            _declarations = declarations;
            _converters = converters;
        }

        public override void AssertLike(Syntax syntax)
        {
            var co = (CompoundSyntax) syntax.Value.Value;
            Tracer.Assert(_list.Length == co.Statements.Length);
            for(var i = 0; i < _list.Length; i++)
                _list[i].AssertLike(co.Statements[i]);

            Tracer.Assert(_declarations.Length == co.Names.Length);
            foreach(var declaration in _declarations)
                declaration.AssertContains(co);

            Tracer.Assert(_converters.Length == co.ConverterStatementPositions.Length);
            for(var i = 0; i < _converters.Length; i++)
                Tracer.Assert(_converters[i] == co.ConverterStatementPositions[i]);
        }
    }

    sealed class Expression : LikeSyntax
    {
        readonly LikeSyntax _s1;
        readonly string _s2;
        readonly LikeSyntax _s3;

        public Expression(LikeSyntax s1, string s2, LikeSyntax s3)
        {
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }

        public override void AssertLike(Syntax syntax)
        {
            var ex = (ExpressionSyntax) syntax.Value.Value;
            AssertLike(_s1, syntax.Left);
            Tracer.Assert(ex.Definable?.Id == _s2);
            AssertLike(_s3, syntax.Right);
        }

        static void AssertLike(LikeSyntax s3, Syntax right)
        {
            if(s3 == null)
                Tracer.Assert(right == null);
            else
                s3.AssertLike(right);
        }
    }

    sealed class Number : LikeSyntax
    {
        readonly long _i;

        internal Number(long i) { _i = i; }

        public override void AssertLike(Syntax syntax)
        {
            var terminalSyntax = (TerminalSyntax) syntax.Value.Value;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(terminalSyntax.ToNumber == _i);
        }
    }
}