using HWClassLibrary.Debug;
using System;
using Reni.Parser;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.FeatureTest.Structure
{
    internal abstract class Syntax
    {
        public static Syntax Number(int i)
        {
            return new Number(i);
        }

        public static Syntax Expression(Syntax s1, string s2, Syntax s3)
        {
            return new Expression(s1, s2, s3);
        }

        public static Syntax Expression(Syntax s1, string s2)
        {
            return new Expression(s1, s2, null);
        }

        public static Syntax Struct()
        {
            return new Struct();
        }

        public abstract void AssertLike(IParsedSyntax syntax);
    }

    internal class Struct : Syntax
    {
        public override void AssertLike(IParsedSyntax syntax)
        {
            var co = (Container)syntax;
        }
    }

    internal class Expression : Syntax
    {
        private readonly Syntax _s1;
        private readonly string _s2;
        private readonly Syntax _s3;

        public Expression(Syntax s1, string s2, Syntax s3)
        {
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }

        public override void AssertLike(IParsedSyntax syntax)
        {
            var ex = (ExpressionSyntax)syntax;
            AssertLike(_s1, ex.Left);
            Tracer.Assert(ex.Token.Name == _s2);
            AssertLike(_s3, ex.Right);
        }

        private static void AssertLike(Syntax s3, ICompileSyntax right)
        {
            if (s3 == null)
                Tracer.Assert(right == null);
            else
                s3.AssertLike((IParsedSyntax)right);
        }
    }

    internal class Number : Syntax
    {
        private readonly Int64 _i;

        internal Number(Int64 i)
        {
            _i = i;
        }

        public override void AssertLike(IParsedSyntax syntax)
        {
            var terminalSyntax = (TerminalSyntax)syntax;
            Tracer.Assert(terminalSyntax.Terminal is Parser.TokenClass.Number);
            Tracer.Assert(Parser.TokenClass.Number.ToInt64(terminalSyntax.Token) == _i);
        }
    }
}

