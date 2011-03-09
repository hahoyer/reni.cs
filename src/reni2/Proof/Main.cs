using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    public static class Main
    {
        public static void Run()
        {
            var statement = new Holder(@"
a elem Integer & 
b elem Integer & 
c elem Integer & 
a^2 + b^2 = c^2 &
a gcd b = 1
");
            statement.Replace("a", "x+y");
        }
    }

    internal abstract class ParsedSyntax : Parser.ParsedSyntax
    {
        protected ParsedSyntax(TokenData token)
            : base(token) { }

        protected ParsedSyntax(TokenData token, int nextObjectId)
            : base(token, nextObjectId) { }

        internal ParsedSyntax Associative<TOperation>(TOperation operation, TokenData token, ParsedSyntax other)
            where TOperation : IAssociative
        {
            var x1 = AssociativeSyntax.ListOf(operation, this);
            var x2 = AssociativeSyntax.ListOf(operation, other);
            if(x1.Apply(x => x2.Apply(y => !IsDistinct(x,y)).Contains(true)).Contains(true))
            {
                NotImplementedMethod(operation, token, other);
                return null;
            }
            return new AssociativeSyntax(operation, token, x1 | x2);
        }

        internal static bool IsDistinct(ParsedSyntax x, ParsedSyntax y)
        {
            if(x.GetType() != y.GetType())
                return true;
            return x.IsDistinct(y);
        }

        protected virtual bool IsDistinct(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return false;
        }

    }

    internal sealed class AssociativeSyntax : ParsedSyntax
    {
        internal readonly IAssociative Operator;
        internal readonly Set<ParsedSyntax> Set;

        public AssociativeSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(token)
        {
            Operator = @operator;
            Set = set;
        }

        internal static Set<ParsedSyntax> ListOf(IAssociative associativeOperator, ParsedSyntax parsedSyntax)
        {
            var commutative = parsedSyntax as AssociativeSyntax;
            if(commutative != null && commutative.Operator == associativeOperator)
                return commutative.Set;
            var result = new Set<ParsedSyntax>();
            result.Add(parsedSyntax);
            return result;
        }
    }

    internal interface IAssociative
    {}


    internal sealed class Element : TokenClass, IPair
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return new PairSyntax(this, left, token, right);
        }
    }

    internal sealed class And : TokenClass, IAssociative
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return left.Associative(this, token, right);
        }
    }

    internal sealed class Sign : TokenClass, IAssociative
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return left.Associative(this, token, right);
        }
    }

    internal sealed class PairSyntax : ParsedSyntax
    {
        internal readonly IPair Operator;
        internal readonly ParsedSyntax Left;
        internal readonly ParsedSyntax Right;

        internal PairSyntax(IPair @operator, ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(token)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        protected override bool IsDistinct(ParsedSyntax other) { return IsDistinct((PairSyntax)other); }
        private bool IsDistinct(PairSyntax other) { return other.Operator != Operator || IsDistinct(other.Left, Left) || IsDistinct(other.Right,Right); }
    }

    internal interface IPair
    {}

    internal sealed class UserSymbol : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new UserSymbolSyntax(token, Name);
        }
    }

    internal sealed class UserSymbolSyntax : ParsedSyntax
    {
        internal readonly string Name;

        public UserSymbolSyntax(TokenData token, string name)
            : base(token) { Name = name; }

        protected override bool IsDistinct(ParsedSyntax other) { return IsDistinct((UserSymbolSyntax)other); }
        private bool IsDistinct(UserSymbolSyntax other) { return Name != other.Name; }
    }

    internal sealed class LeftParenthesis : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left == null);
            return new LeftParenthesisSyntax(token, right);
        }
    }

    internal sealed class RightParenthesis : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            var leftParenthesisSyntax = left as LeftParenthesisSyntax;
            Tracer.Assert(leftParenthesisSyntax != null);
            Tracer.Assert(leftParenthesisSyntax.Right != null);
            return leftParenthesisSyntax.Right;
        }
    }

    internal sealed class LeftParenthesisSyntax : ParsedSyntax
    {
        public readonly ParsedSyntax Right;

        public LeftParenthesisSyntax(TokenData token, ParsedSyntax right)
            : base(token) { Right = right; }

    }

    internal class TypeOperator : TokenClass
    {}

    internal sealed class Integer : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new IntegerSyntax(token);
        }
    }

    internal sealed class IntegerSyntax : ParsedSyntax
    {
        public IntegerSyntax(TokenData token)
            : base(token) { }
    }

    internal sealed class Number : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new NumberSyntax(token);
        }
    }

    internal sealed class NumberSyntax : ParsedSyntax
    {
        internal BigInteger Value;

        internal NumberSyntax(TokenData token)
            : base(token) { Value = BigInteger.Parse(token.Name); }
    }

    internal sealed class Equal : TokenClass, IPair
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return new PairSyntax(this, left, token, right);
        }
    }
    internal sealed class GreatesCommonDenominator : TokenClass, IPair
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return new PairSyntax(this, left, token, right);
        }
    }

    internal sealed class Caret : TokenClass, IPair
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return new PairSyntax(this, left, token, right);
        }
    }

    internal class KGV : TokenClass
    { }

    internal class Star : TokenClass
    {}

    internal class Slash : TokenClass
    {}

    internal class Exclamation : TokenClass
    {}

    internal class NotEqual : TokenClass
    {}

    internal class CompareOperator : TokenClass
    {}

}