using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory
{
    class InfixHandler : DumpableObject, IValueProvider
    {
        abstract class InfixTypeErrorToken : DumpableObject, ITokenClass, IErrorToken
        {
            [EnableDump]
            protected readonly Issue Issue;

            protected InfixTypeErrorToken(BinaryTree target,  IssueId issueId) 
                => Issue = new Issue(issueId, target.Token.Characters, GetMessage(target.TokenClass));

            static string GetMessage(ITokenClass tokenClass)
            {
                var types = new List<string>();
                if(tokenClass is IInfix)
                    types.Add("infix");
                if(tokenClass is ISuffix)
                    types.Add("suffix");
                if(tokenClass is IPrefix)
                    types.Add("prefix");
                if(tokenClass is ITerminal)
                    types.Add("terminal");

                return $"(actual: {types.Stringify(", ")})";
            }

            IssueId IErrorToken.IssueId => Issue.IssueId;

            string ITokenClass.Id => "InfixTypeErrorToken";
        }

        sealed class InfixErrorToken : InfixTypeErrorToken, IInfix
        {
            public InfixErrorToken(BinaryTree target)
                : base(target, IssueId.InvalidInfixExpression) { }

            Result IInfix.Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            {
                NotImplementedMethod(context, category, left, right);
                return default;
            }
        }

        sealed class SuffixErrorToken : InfixTypeErrorToken, ISuffix
        {
            public SuffixErrorToken(BinaryTree target)
                : base(target, IssueId.InvalidSuffixExpression) { }

            Result ISuffix.Result(ContextBase context, Category category, ValueSyntax left)
                => new Result(category, Issue);
        }

        sealed class PrefixErrorToken : InfixTypeErrorToken, IPrefix
        {
            public PrefixErrorToken(BinaryTree target)
                : base(target, IssueId.InvalidPrefixExpression) { }

            Result IPrefix.Result(ContextBase context, Category category, ValueSyntax right, IToken token)
            {
                NotImplementedMethod(context, category, right, token);
                return default;
            }
        }

        sealed class TerminalErrorToken : InfixTypeErrorToken, ITerminal
        {
            public TerminalErrorToken(BinaryTree target)
                : base(target, IssueId.InvalidTerminalExpression) { }

            Result ITerminal.Result(ContextBase context, Category category, IToken token)
            {
                NotImplementedMethod(context, category, token);
                return default;
            }

            ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor)
            {
                NotImplementedMethod(visitor);
                return default;
            }
        }

        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor anchor)
        {
            var left = factory.GetValueSyntax(target.Left);
            var right = factory.GetValueSyntax(target.Right);

            var tokenClass = GetTokenClass(target, left != null, right != null);

            return left
                .GetInfixSyntax(tokenClass, target.Token, right, Anchor.Create(target).Combine(anchor));
        }

        static ITokenClass GetTokenClass(BinaryTree target, bool left, bool right)
            => left
                ? right
                    ? GetTokenClass<IInfix>(target)
                    : GetTokenClass<ISuffix>(target)
                : right
                    ? GetTokenClass<IPrefix>(target)
                    : GetTokenClass<ITerminal>(target);

        static ITokenClass GetTokenClass<TInfixType>(BinaryTree target)
        {
            var tokenClass = target.TokenClass;
            if(tokenClass is TInfixType)
                return tokenClass;

            return GetErrorTokenClass<TInfixType>(target);
        }

        static ITokenClass GetErrorTokenClass<TInfixType>(BinaryTree target)
        {
            if(typeof(TInfixType).Is<IInfix>())
                return new InfixErrorToken(target);
            if(typeof(TInfixType).Is<ISuffix>())
                return new SuffixErrorToken(target);
            if(typeof(TInfixType).Is<IPrefix>())
                return new PrefixErrorToken(target);
            if(typeof(TInfixType).Is<ITerminal>())
                return new TerminalErrorToken(target);
            throw new InvalidOperationException();
        }
    }
}