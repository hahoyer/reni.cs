using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
    internal abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(Token token)
            : base(token) {}

    }

    internal sealed class TerminalSyntax : SpecialSyntax
    {
        [Node]
        internal readonly Terminal Terminal;

        public TerminalSyntax(Token token, Terminal terminal)
            : base(token)
        {
            Terminal = terminal;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Terminal.Result(context, category, Token);
        }
    }

    internal sealed class PrefixSyntax : SpecialSyntax
    {
        [Node]
        internal readonly Prefix Prefix;
        [Node]
        internal readonly ICompileSyntax Right;

        public PrefixSyntax(Token token, Prefix prefix, ICompileSyntax right)
            : base(token)
        {
            Prefix = prefix;
            Right = right;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Prefix.Result(context, category, Token, Right);
        }

        protected internal override string DumpShort()
        {
            return base.DumpShort() + "(" + Right.DumpShort() + ")";
        }
    }

    internal sealed class InfixSyntax : SpecialSyntax
    {
        [Node]
        internal readonly ICompileSyntax Left;
        [Node]
        internal readonly Infix Infix;
        [Node]
        internal readonly ICompileSyntax Right;

        public InfixSyntax(Token token, ICompileSyntax left, Infix infix, ICompileSyntax right) : base(token)
        {
            Left = left;
            Infix = infix;
            Right = right;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Infix.Result(context, category, Left, Token, Right);
        }

        protected internal override string DumpShort()
        {
            return "("+Left.DumpShort() + ")"+base.DumpShort()+"("+Right.DumpShort()+")";
        }
    }
}
