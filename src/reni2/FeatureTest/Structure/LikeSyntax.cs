using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using System;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.FeatureTest.Structure
{
    internal abstract class LikeSyntax
    {
        public static LikeSyntax Number(int i) { return new Number(i); }

        public LikeSyntax Expression(string s2, LikeSyntax s3) { return new Expression(this, s2, s3); }

        public static LikeSyntax Expression(LikeSyntax s1, string s2, LikeSyntax s3) { return new Expression(s1, s2, s3); }

        public LikeSyntax Expression(string s2) { return new Expression(this, s2, null); }

        public static LikeSyntax Struct(LikeSyntax[] list, Declaration[] declarations, int[] converters, string[] properties) { return new Struct(list, declarations, converters, properties); }

        public abstract void AssertLike(IParsedSyntax syntax);

        public static LikeSyntax operator +(LikeSyntax x, LikeSyntax y) { return x.Expression("+", y); }
        public static LikeSyntax operator -(LikeSyntax x, LikeSyntax y) { return x.Expression("-", y); }
        public static LikeSyntax operator *(LikeSyntax x, LikeSyntax y) { return x.Expression("*", y); }
        public static LikeSyntax operator /(LikeSyntax x, LikeSyntax y) { return x.Expression("/", y); }
        public LikeSyntax dump_print { get { return Expression("dump_print"); } }

        public static LikeSyntax Null { get { return new Empty(); } }

        public static Declaration Declaration(string name, int position) { return new Declaration(name, position); }

        public static LikeSyntax Symbol(string s) { return new Expression(null, s, null); }
    }

    internal class Empty : LikeSyntax
    {
        public override void AssertLike(IParsedSyntax syntax) { Tracer.Assert(syntax is EmptyList); }
    }

    internal class Declaration
    {
        private readonly string _name;
        private readonly int _position;

        public Declaration(string name, int position)
        {
            _name = name;
            _position = position;
        }

        public void AssertLike(KeyValuePair<string, int> item)
        {
            Tracer.Assert(_name == item.Key);
            Tracer.Assert(_position == item.Value);
        }
    }

    internal class Struct : LikeSyntax
    {
        private readonly LikeSyntax[] _list;
        private readonly Declaration[] _declarations;
        private readonly int[] _converters;
        private readonly string[] _properties;

        public Struct(LikeSyntax[] list, Declaration[] declarations, int[] converters, string[] properties)
        {
            _list = list;
            _declarations = declarations;
            _converters = converters;
            _properties = properties;
        }

        public override void AssertLike(IParsedSyntax syntax)
        {
            var co = (Container) syntax;
            Tracer.Assert(_list.Length == co.List.Length);
            for(var i = 0; i < _list.Length; i++)
                _list[i].AssertLike((IParsedSyntax) co.List[i]);
            Tracer.Assert(_declarations.Length == co.Dictionary.Count);
            var coi = co.Dictionary.GetEnumerator();
            coi.MoveNext();
            for(var i = 0; i < _declarations.Length; i++, coi.MoveNext())
                _declarations[i].AssertLike(coi.Current);
            Tracer.Assert(_converters.Length == co.Converters.Length);
            for(var i = 0; i < _converters.Length; i++)
                Tracer.Assert(_converters[i] == co.Converters[i]);
            Tracer.Assert(_properties.Length == co.Properties.Length);
            for(var i = 0; i < _properties.Length; i++)
                Tracer.Assert(_properties[i] == co.Properties[i]);
        }
    }

    internal class Expression : LikeSyntax
    {
        private readonly LikeSyntax _s1;
        private readonly string _s2;
        private readonly LikeSyntax _s3;

        public Expression(LikeSyntax s1, string s2, LikeSyntax s3)
        {
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }

        public override void AssertLike(IParsedSyntax syntax)
        {
            var ex = (ExpressionSyntax) syntax;
            AssertLike(_s1, ex.Left);
            Tracer.Assert(ex.Token.Name == _s2);
            AssertLike(_s3, ex.Right);
        }

        private static void AssertLike(LikeSyntax s3, ICompileSyntax right)
        {
            if(s3 == null)
                Tracer.Assert(right == null);
            else
                s3.AssertLike((IParsedSyntax) right);
        }
    }

    internal class Number : LikeSyntax
    {
        private readonly Int64 _i;

        internal Number(Int64 i) { _i = i; }

        public override void AssertLike(IParsedSyntax syntax)
        {
            var terminalSyntax = (TerminalSyntax) syntax;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(TokenClasses.Number.ToInt64(terminalSyntax.Token) == _i);
        }
    }
}