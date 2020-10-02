using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace ReniUI.Helper
{
    sealed class Syntax : SyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> LocatePosition;
            public FunctionCache<int, Syntax> LocatePositionExtended;
        }

        readonly CacheContainer Cache = new CacheContainer();

        public Syntax(Reni.TokenClasses.Syntax target, Syntax parent = null)
            : base(target, parent)
        {
            Cache.LocatePosition = new FunctionCache<int, Syntax>(LocatePositionForCache);
            Cache.LocatePositionExtended = new FunctionCache<int, Syntax>(LocatePositionExtendedForCache);
        }

        [DisableDump]
        internal IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        [DisableDump]
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        [DisableDump]
        internal string[] DeclarationOptions => Declarations.Distinct().ToArray();

        Result<Value> Value => Target.Value;
        Result<Statement[]> Statements => Target.Option.Statements;
        Result<Statement> Statement => Target.Option.Statement;
        Result<Declarer> Declarer => Target.Option.Declarer;

        [DisableDump]
        IEnumerable<string> Declarations
        {
            get
            {
                if(Value != null)
                    return Contexts
                        .SelectMany(item => Value.Target.DeclarationOptions(item));

                if(Statements != null || Statement != null)
                    return Contexts
                        .SelectMany(item => item.DeclarationOptions)
                        .Concat(DeclarationTagToken.DeclarationOptions);

                if(Declarer != null ||
                   Target.TokenClass is LeftParenthesis ||
                   Target.TokenClass is DeclarationTagToken)
                    return new string[0];

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        ContextBase[] Contexts
        {
            get
            {
                if(IsStatementsLevel)
                    return ((CompoundSyntax)Target.Value.Target)
                        .ResultCache
                        .Values
                        .Select(item => item.Type.ToContext)
                        .ToArray();

                var parentContexts = Parent.Contexts;

                if(IsFunctionLevel)
                    return parentContexts
                        .SelectMany(context => FunctionContexts(context, Value.Target))
                        .ToArray();

                return parentContexts;
            }
        }

        bool IsStatementsLevel
        {
            get
            {
                if(TokenClass is EndOfText)
                    return true;

                if(Value != null)
                    return false;
                if(Statements != null)
                    return Parent?.TokenClass != TokenClass;
                if(Statement != null)
                    return false;

                if(TokenClass is LeftParenthesis)
                    return false;

                if(TokenClass is BeginOfText)
                    return false;

                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        bool IsFunctionLevel => TokenClass is Reni.TokenClasses.Function;

        public Syntax LocatePosition(int current) => Cache.LocatePosition[current];

        protected override Syntax Create(Reni.TokenClasses.Syntax target, Syntax parent)
            => new Syntax(target, parent);

        internal Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        IEnumerable<Syntax> GetItems()
        {
            if(Left != null)
                foreach(var sourceSyntax in Left.Items)
                    yield return sourceSyntax;

            yield return this;

            if(Right != null)
                foreach(var sourceSyntax in Right.Items)
                    yield return sourceSyntax;
        }

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        Syntax LocatePositionExtended(int current) => Cache.LocatePositionExtended[current];

        Syntax LocatePositionForCache(int current)
            => Left?.CheckedLocatePosition(current) ??
               Right?.CheckedLocatePosition(current) ??
               this;

        Syntax LocatePositionExtendedForCache(int current)
            => Contains(current)
                ? Left?.CheckedLocatePositionExtended(current) ??
                  Right?.CheckedLocatePositionExtended(current) ??
                  this
                : Parent.LocatePositionExtendedForCache(current);

        Syntax CheckedLocatePosition(int current)
            =>
                Contains(current)
                    ? LocatePosition(current)
                    : null;

        Syntax CheckedLocatePositionExtended(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;

        static IEnumerable<ContextBase> FunctionContexts(ContextBase context, Value body)
            => ((FunctionBodyType)context.ResultCache(body).Type)
                .Functions
                .Select(item => item.CreateSubContext(false));
    }
}