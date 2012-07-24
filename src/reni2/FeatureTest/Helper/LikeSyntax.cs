#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.FeatureTest.Helper
{
    abstract class LikeSyntax
    {
        public static LikeSyntax Number(int i) { return new Number(i); }

        public LikeSyntax Expression(string s2, LikeSyntax s3) { return new Expression(this, s2, s3); }

        public static LikeSyntax Expression(LikeSyntax s1, string s2, LikeSyntax s3) { return new Expression(s1, s2, s3); }

        public LikeSyntax Expression(string s2) { return new Expression(this, s2, null); }

        public static LikeSyntax Struct(LikeSyntax[] list, Declaration[] declarations, int[] converters) { return new Struct(list, declarations, converters); }

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

    sealed class Empty : LikeSyntax
    {
        public override void AssertLike(IParsedSyntax syntax) { Tracer.Assert(syntax is EmptyList); }
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

        public void AssertLike(KeyValuePair<string, int> item)
        {
            Tracer.Assert(_name == item.Key);
            Tracer.Assert(_position == item.Value);
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

        public override void AssertLike(IParsedSyntax syntax)
        {
            var co = (Container) syntax;
            Tracer.Assert(_list.Length == co.Statements.Length);
            for(var i = 0; i < _list.Length; i++)
                _list[i].AssertLike(co.Statements[i]);
            Tracer.Assert(_declarations.Length == co.Dictionary.Count);
            var coi = co.Dictionary.GetEnumerator();
            coi.MoveNext();
            for(var i = 0; i < _declarations.Length; i++, coi.MoveNext())
                _declarations[i].AssertLike(coi.Current);
            Tracer.Assert(_converters.Length == co.Converters.Length);
            for(var i = 0; i < _converters.Length; i++)
                Tracer.Assert(_converters[i] == co.Converters[i]);
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

        public override void AssertLike(IParsedSyntax syntax)
        {
            var ex = (ExpressionSyntax) syntax;
            AssertLike(_s1, ex.Left);
            Tracer.Assert(ex.Token.Name == _s2);
            AssertLike(_s3, ex.Right);
        }

        static void AssertLike(LikeSyntax s3, CompileSyntax right)
        {
            if(s3 == null)
                Tracer.Assert(right == null);
            else
                s3.AssertLike(right);
        }
    }

    sealed class Number : LikeSyntax
    {
        readonly Int64 _i;

        internal Number(Int64 i) { _i = i; }

        public override void AssertLike(IParsedSyntax syntax)
        {
            var terminalSyntax = (TerminalSyntax) syntax;
            Tracer.Assert(terminalSyntax.Terminal is TokenClasses.Number);
            Tracer.Assert(TokenClasses.Number.ToInt64(terminalSyntax.Token) == _i);
        }
    }
}