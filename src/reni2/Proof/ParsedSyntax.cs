using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal abstract class ParsedSyntax : Parser.ParsedSyntax
    {
        protected ParsedSyntax(TokenData token)
            : base(token) { }

        [IsDumpEnabled(false)]
        internal virtual Set<string> Variables
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal ParsedSyntax Associative<TOperation>(TOperation operation, TokenData token, ParsedSyntax other)
            where TOperation : IAssociative
        {
            var x1 = AssociativeSyntax.ListOf(operation, this);
            var x2 = AssociativeSyntax.ListOf(operation, other);
            if(x2.IsDistinct(x1))
                return new AssociativeSyntax(operation, token, x1 | x2);
            NotImplementedMethod(operation, token, other);
            return null;
        }

        internal virtual bool IsDistinct(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return false;
        }
    }

    static class ParsedSyntaxExtender
    {
        internal static bool IsDistinct(this IEnumerable<ParsedSyntax> x2, IEnumerable<ParsedSyntax> x1)
        {
            return x1.All(x => x2.All(xx => IsDistinct(x, xx)));
        }

        internal static bool IsDistinct(ParsedSyntax x, ParsedSyntax y)
        {
            if (x.GetType() != y.GetType())
                return true;
            return x.IsDistinct(y);
        }
    }

}
