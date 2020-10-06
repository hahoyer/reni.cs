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
        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Left == null && binaryTree.Right == null)
                return new TerminalSyntax(this, binaryTree);

            if(binaryTree.Left != null && binaryTree.Right == null)
            {
                return new TerminalSyntax(this, binaryTree)
                    .Issues<ValueSyntax>
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

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        protected ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        protected override ISyntaxFactory Provider => SyntaxFactory.Terminal;

    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueProvider
    {
        Result IInfix.Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result
            (ContextBase context, Category category, ValueSyntax right, BinaryTree token)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right);

        protected abstract Result Result
            (ContextBase context, Category category, ValueSyntax right);

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
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
            (ContextBase context, Category category, ValueSyntax right, BinaryTree token)
            => Result(context, category, right, token);

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax right, BinaryTree token);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
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
        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Right == null)
                return SuffixSyntax.Create(binaryTree.Left.Syntax(scope), this, binaryTree);

            NotImplementedMethod(binaryTree);
            return null;
        }

        Result ISuffix.Result(ContextBase context, Category category, ValueSyntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, ValueSyntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix, IValueProvider
    {
        Result IInfix.Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax left, ValueSyntax right);

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
            => InfixSyntax.Create(binaryTree.Left.Syntax(scope), this, binaryTree.Right.Syntax(scope), binaryTree);
    }
}