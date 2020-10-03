using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass {}

    abstract class InfixPrefixToken : TokenClass {}

    abstract class NonSuffixToken : TokenClass {}

    abstract class SuffixToken : TokenClass {}

    abstract class InfixToken : TokenClass {}

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal, IValueProvider
    {
        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Left == null && binaryTree.Right == null)
                return new TerminalSyntax(this, binaryTree);

            if(binaryTree.Left != null && binaryTree.Right == null)
            {
                return new TerminalSyntax(this, binaryTree)
                    .Issues<Syntax>
                    (
                    IssueId.TerminalUsedAsSuffix
                    .Issue(binaryTree.Left.SourcePart));
            }

            NotImplementedMethod(binaryTree);
            return null;
        }

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        Syntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        protected Syntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueProvider
    {
        Result IInfix.Result
            (ContextBase context, Category category, Syntax left, Syntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result
            (ContextBase context, Category category, Syntax right, BinaryTree token)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, Syntax left, Syntax right);

        protected abstract Result Result
            (ContextBase context, Category category, Syntax right);

        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Left != null && binaryTree.Right != null)
                return InfixSyntax.Create(binaryTree.Left.Syntax(scope), this, binaryTree.Right.Syntax(scope), binaryTree);

            if(binaryTree.Left == null && binaryTree.Right != null)
                return PrefixSyntax.Create(this, binaryTree.Right.Syntax(scope), binaryTree);

            NotImplementedMethod(binaryTree);
            return null;
        }
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix, IValueProvider
    {
        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category);

        protected abstract Result Result
            (ContextBase context, Category category);

        Result IPrefix.Result
            (ContextBase context, Category category, Syntax right, BinaryTree token)
            => Result(context, category, right, token);

        protected abstract Result Result
            (ContextBase callContext, Category category, Syntax right, BinaryTree token);

        Syntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual Syntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Left == null)
            {
                if(binaryTree.Right == null)
                    return new TerminalSyntax(this, binaryTree);

                return PrefixSyntax.Create(this, binaryTree.Right.Syntax(scope), binaryTree);
            }

            NotImplementedMethod(binaryTree);
            return null;
        }
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix, IValueProvider
    {
        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Right == null)
                return SuffixSyntax.Create(binaryTree.Left.Syntax(scope), this, binaryTree);

            NotImplementedMethod(binaryTree);
            return null;
        }

        Result ISuffix.Result(ContextBase context, Category category, Syntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, Syntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix, IValueProvider
    {
        Result IInfix.Result
            (ContextBase context, Category category, Syntax left, Syntax right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, Syntax left, Syntax right);

        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
            => InfixSyntax.Create(binaryTree.Left.Syntax(scope), this, binaryTree.Right.Syntax(scope), binaryTree);
    }
}