using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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

        protected readonly ICompileSyntax Else;

        protected CondSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax, ICompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var elseType = context.CondBranchType(Else);
            var thenType = context.CondBranchType(Then);

            var commonType = TypeBase.CommonType(thenType, elseType);
            if(category <= (Category.Type | Category.Size))
                return commonType.CreateResult(category);

            var condResult = context
                .Result(category | Category.Type, Cond)
                .ConvertTo(TypeBase.CreateBit);

            if(thenType.IsPending)
                return context.CondBranchResult(condResult.Refs, category, Else);
            if (elseType.IsPending)
                return context.CondBranchResult(condResult.Refs, category, Then);
            
            var thenRawResult = context.Result(category | Category.Type, Then).AutomaticDereference();
            var elseRawResult = context.Result(category | Category.Type, Else).AutomaticDereference();

            var thenResult = thenType
                .Conversion(category | Category.Type, commonType)
                .UseWithArg(thenRawResult)
                .CreateStatement(category, context.RefAlignParam);
            var elseResult = elseType
                .Conversion(category | Category.Type, commonType)
                .UseWithArg(elseRawResult)
                .CreateStatement(category, context.RefAlignParam);

            return commonType.CreateResult
                (
                category,
                () => condResult.Code.CreateThenElse(thenResult.Code, elseResult.Code),
                () => condResult.Refs.CreateSequence(thenResult.Refs).CreateSequence(elseResult.Refs)
                );
        }

        internal protected override string DumpShort() { return "(" + Cond.DumpShort() + ")then(" + Then.DumpShort() + ")"; }
    }

    [Serializable]
    internal sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax)
            : base(condSyntax, thenToken, thenSyntax, null) { }

        internal protected override IParsedSyntax CreateElseSyntax(Token token, ICompileSyntax elseSyntax) { return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax); }
    }

    [Serializable]
    internal sealed class ThenElseSyntax : CondSyntax
    {
        [Node]
        private readonly Token ElseToken;

        public ThenElseSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax, Token elseToken, ICompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax, elseSyntax)
        {
            ElseToken = elseToken;
        }

        internal protected override string DumpShort() { return base.DumpShort() + "else(" + 
            Else.DumpShort() + ")"; }
    }
}