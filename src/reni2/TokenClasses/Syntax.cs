using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class Syntax : DumpableObject, ISourcePartProxy, ISyntax, ValueCache.IContainer, ITree<Syntax>
    {
        static int NextObjectId;

        internal static Syntax Create
        (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right
        )
            => new Syntax(left, tokenClass, token, right);

        [DisableDump]
        internal readonly IToken Token;
        [DisableDump]
        internal SyntaxOption Option {get;}

        Syntax ITree<Syntax>.Left => Left;
        Syntax ITree<Syntax>.Right => Right;


        [EnableDumpExcept(null)]
        internal Syntax Left {get;}

        [EnableDumpExcept(null)]
        internal Syntax Right {get;}

        internal ITokenClass TokenClass {get;}
        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        Syntax
        (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right
        )
            : base(NextObjectId++)
        {
            Token = token;
            Left = left;
            TokenClass = tokenClass;
            Right = right;

            Option = new SyntaxOption(this);
        }

        SourcePart ISourcePartProxy.All => SourcePart;
        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;

        [DisableDump]
        internal IDefaultScopeProvider DefaultScopeProvider
            => TokenClass as IDefaultScopeProvider ?? Option.Parent?.DefaultScopeProvider;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.Characters + Right?.SourcePart;

        [DisableDump]
        public string[] DeclarationOptions
            => Option
                .Declarations
                .Distinct()
                .ToArray();

        [DisableDump]
        internal Result<Value> Value => this.CachedValue(() => GetValue(this));

        [DisableDump]
        internal Result<Declarator> Declarer
        {
            get
            {
                switch(TokenClass)
                {
                    case IDeclaratorTokenClass tokenClass: return tokenClass.Get(this);
                    case RightParenthesis _:
                    case EndOfText _:
                    case List _:
                    case ScannerSyntaxError _: return null;
                }

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal Result<Statement[]> ForceStatements => this.CachedValue(() => GetStatements());

        [DisableDump]
        internal IEnumerable<Issue> Issues => Option.Issues;

        [DisableDump]
        internal IEnumerable<Issue> AllIssues
            => Left?.AllIssues
                .plus(Issues)
                .plus(Right?.AllIssues);

        internal Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part) ? Locate(part) : null;

        internal IEnumerable<Syntax> Belongings(Syntax recent)
        {
            var root = RootOfBelongings(recent);

            return root?.TokenClass is IBelongingsMatcher matcher
                ? root
                    .ItemsAsLongAs(item => matcher.IsBelongingTo(item.TokenClass))
                    .ToArray()
                : null;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));


        internal Syntax RootOfBelongings(Syntax recent)
        {
            if(!(recent.TokenClass is IBelongingsMatcher matcher))
                return null;

            var sourceSyntaxs = BackChain(recent)
                .ToArray();

            return sourceSyntaxs
                       .Skip(1)
                       .TakeWhile(item => matcher.IsBelongingTo(item.TokenClass))
                       .LastOrDefault() ??
                   recent;
        }

        IEnumerable<Syntax> BackChain(Syntax recent)
        {
            var subChain = SubBackChain(recent);
            if(subChain == null)
                yield break;

            foreach(var items in subChain)
                yield return items;

            yield return this;
        }

        Syntax[] SubBackChain(Syntax recent)
        {
            if(this == recent)
                return new Syntax[0];

            if(Left != null)
            {
                var result = Left.BackChain(recent).ToArray();
                if(result.Any())
                    return result;
            }

            if(Right != null)
            {
                var result = Right.BackChain(recent).ToArray();
                if(result.Any())
                    return result;
            }

            return null;
        }

        Result<Value> GetValue(Syntax syntax)
        {
            var value = Option.Value;
            if(value != null)
                return value;

            var statement = Option.Statement;
            if(statement != null)
                return CompoundSyntax.Create(statement, syntax);

            var statements = Option.Statements;
            if(statements != null)
                return CompoundSyntax.Create(statements, syntax);

            return IssueId.InvalidExpression.Value(syntax);
        }

        internal Result<Statement[]> GetStatements(List type = null)
        {
            var statements = Option.GetStatements(type);
            if(statements != null)
                return statements;

            var statement = Option.Statement;
            if(statement != null)
                return statement.Convert(x => new[] {x});

            var value = Option.Value;
            if(value != null)
                return Statement.CreateStatements(value, Option.DefaultScopeProvider);

            return new Result<Statement[]>
                (new Statement[0], IssueId.InvalidListOperandSequence.Issue(SourcePart));
        }

        internal Result<Syntax> GetBracketKernel(int level, Syntax parent)
        {
            Tracer.Assert(parent.Right == null);

            if(!(TokenClass is LeftParenthesis leftParenthesis))
                return new Result<Syntax>(this, IssueId.ExtraRightBracket.Issue(parent.SourcePart));

            Tracer.Assert(Left == null);

            var levelDelta = leftParenthesis.Level - level;

            if(levelDelta == 0)
                return Right;

            if(levelDelta > 0)
                return new Result<Syntax>
                    (Right, IssueId.ExtraLeftBracket.Issue(SourcePart));

            NotImplementedMethod(level, parent);
            return null;
        }

        public bool IsEqual(Syntax other, IComparator differenceHandler)
        {
            if(TokenClass != other.TokenClass)
                return false;

            NotImplementedMethod(other, differenceHandler);
            return false;
        }

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }
}