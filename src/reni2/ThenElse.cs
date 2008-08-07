using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    [Serializable]
    internal abstract class CondSyntax : CompileSyntax
    {
        protected readonly ICompileSyntax Cond;
        protected readonly ICompileSyntax Then;

        protected CondSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax) : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var condResult = Cond.Result(context, category | Category.Type);
            condResult = condResult.Type.Conversion(category, TypeBase.CreateBit)
                .UseWithArg(condResult);

            var thenResult = Then.Result(context, category | Category.Type).AutomaticDereference();
            var elseResult = CreateElseResult(context, category).AutomaticDereference();

            if(thenResult.Type.IsPending)
                return elseResult.Type.ThenElseWithPending(category, condResult.Refs, elseResult.Refs);
            if(elseResult.Type.IsPending)
                return thenResult.Type.ThenElseWithPending(category, condResult.Refs, thenResult.Refs);

            var commonType = thenResult.Type.CommonType(elseResult.Type);

            thenResult = thenResult.Type.Conversion(category, commonType).UseWithArg(thenResult).CreateStatement();
            elseResult = elseResult.Type.Conversion(category, commonType).UseWithArg(elseResult).CreateStatement();

            return commonType.CreateResult
                (
                category,
                () => condResult.Code.CreateThenElse(thenResult.Code, elseResult.Code),
                () => condResult.Refs.Pair(thenResult.Refs).Pair(elseResult.Refs),
                () => condResult.Internal
                );
        }

        protected abstract Result CreateElseResult(ContextBase context, Category category);

        internal protected override string DumpShort()
        {
            return "(" + Cond.DumpShort() + ")then(" + Then.DumpShort() + ")";
        }
    }

    [Serializable]
    internal sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax) : base(condSyntax, thenToken, thenSyntax) {}

        protected override Result CreateElseResult(ContextBase context, Category category)
        {
            return TypeBase.CreateVoidResult(category | Category.Type);
        }

        internal protected override IParsedSyntax CreateElseSyntax(Token token, ICompileSyntax elseSyntax)
        {
            return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax);
        }
    }

    [Serializable]
    internal sealed class ThenElseSyntax : CondSyntax
    {
        private readonly Token ElseToken;
        private readonly ICompileSyntax Else;

        public ThenElseSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax, Token elseToken, ICompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax)
        {
            ElseToken = elseToken;
            Else = elseSyntax;
        }

        protected override Result CreateElseResult(ContextBase context, Category category)
        {
            return Else.Result(context, category | Category.Type);
        }

        internal protected override string DumpShort()
        {
            return base.DumpShort() + "else(" + Else.DumpShort() + ")";
        }
    }
}