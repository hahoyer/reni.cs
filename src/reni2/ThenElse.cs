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
        internal readonly ICompileSyntax Cond;

        [Node]
        internal readonly ICompileSyntax Then;

        [Node]
        internal readonly ICompileSyntax Else;

        protected CondSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax,
                             ICompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var trace = false;
            StartMethodDump(trace, context, category);
            return ReturnMethodDump(trace, InternalResult(context, category));
        }

        internal Result CondResult(ContextBase context, Category category) { return context.Result(category | Category.Type, Cond).ConvertTo(TypeBase.CreateBit) & category; }

        private Result ElseResult(ContextBase context, Category category)
        {
            if (Else == null)
                return TypeBase.CreateVoid.CreateResult(category);
            return CondBranchResult(context,category, Else);
        }

        private Result ThenResult(ContextBase context, Category category) { return CondBranchResult(context, category, Then); }

        private Result CondBranchResult(ContextBase context, Category category, ICompileSyntax syntax)
        {
            var branchResult = context.Result(category | Category.Type, syntax).AutomaticDereference();
            if ((category - Category.Type).IsNull)
                return branchResult;

            var commonType = context.CommonType(this);
            return branchResult.Type
                .Conversion(category | Category.Type, commonType)
                .UseWithArg(branchResult)
                .CreateStatement(category, context.RefAlignParam);
        }

        private Result InternalResult(ContextBase context, Category category)
        {
            var commonType = context.CommonType(this);
            if(category <= (Category.Type | Category.Size))
                return commonType.CreateResult(category);

            var condResult = CondResult(context, category);
            return commonType.CreateResult
                (
                category,
                () => condResult.Code.CreateThenElse(ThenResult(context, Category.Code).Code, ElseResult(context, Category.Code).Code),
                () => condResult.Refs + context.CommonRefs(this)
                );
        }

        internal Result CommonResult(ContextBase context, Category category, bool thenIsPending, bool elseIsPending)
        {
            if(!thenIsPending)
                return ThenResult(context,category);
            if(!elseIsPending)
                return ElseResult(context,category);
            NotImplementedMethod(context,category, thenIsPending,elseIsPending);
            return null;                   
        }

        internal Result CommonResult(ContextBase context, Category category)
        {
            Tracer.Assert(category <= (Category.Type|Category.Refs));
            var thenResult = ThenResult(context, category);
            var elseResult = ElseResult(context, category);
            var result = new Result();
            if (category.HasType)
                result.Type = TypeBase.CommonType(thenResult.Type,elseResult.Type);
            if (category.HasRefs)
                result.Refs = thenResult.Refs + elseResult.Refs;
            return result;
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

        public ThenElseSyntax(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax, Token elseToken,
                              ICompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax, elseSyntax) { ElseToken = elseToken; }

        internal protected override string DumpShort()
        {
            return base.DumpShort() + "else(" +
                   Else.DumpShort() + ")";
        }
    }
}