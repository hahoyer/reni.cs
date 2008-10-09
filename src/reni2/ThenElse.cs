using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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
        [Node]
        protected readonly ICompileSyntax Cond;
        [Node]
        protected readonly ICompileSyntax Then;

        protected CondSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax) : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var thenType = context.Type(Then).AutomaticDereference();
            var elseType = ElseResult(context, Category.Type).Type.AutomaticDereference();
            var commonType = thenType.CommonType(elseType);
            if(category <= (Category.Type | Category.Size))
                return commonType.CreateResult(category);

            var condResult = context.ConvertToSequence(category | Category.Type, Cond, TypeBase.CreateBit,1);

            var branchCategory = category | Category.Internal | Category.Type;
            var thenResult = context.Result(branchCategory, Then).AutomaticDereference();
            var elseResult = ElseResult(context, branchCategory).AutomaticDereference();

            if(thenType.IsPending)
                return elseType.ThenElseWithPending(branchCategory, condResult.Refs, elseResult.Refs);
            if(elseType.IsPending)
                return thenType.ThenElseWithPending(branchCategory, condResult.Refs, thenResult.Refs);

            thenResult = thenType.Conversion(branchCategory, commonType).UseWithArg(thenResult).CreateStatement(category);
            elseResult = elseType.Conversion(branchCategory, commonType).UseWithArg(elseResult).CreateStatement(category);

            return commonType.CreateResult
                (
                category,
                () => condResult.Code.CreateThenElse(thenResult.Code, elseResult.Code),
                () => condResult.Refs.CreateSequence(thenResult.Refs).CreateSequence(elseResult.Refs),                                                  
                () => condResult.Internal
                );
        }

        protected abstract Result ElseResult(ContextBase context, Category category);

        internal protected override string DumpShort()
        {
            return "(" + Cond.DumpShort() + ")then(" + Then.DumpShort() + ")";
        }
    }

    [Serializable]                               
    internal sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax) : base(condSyntax, thenToken, thenSyntax) {}

        protected override Result ElseResult(ContextBase context, Category category)
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
        [Node]
        private readonly Token ElseToken;
        [Node]
        private readonly ICompileSyntax Else;

        public ThenElseSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax, Token elseToken, ICompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax)
        {
            ElseToken = elseToken;
            Else = elseSyntax;
        }

        protected override Result ElseResult(ContextBase context, Category category)
        {
            return context.Result(category | Category.Type, Else);
        }

        internal protected override string DumpShort()
        {
            return base.DumpShort() + "else(" + Else.DumpShort() + ")";
        }
    }
}