using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
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

        protected CondSyntax(ICompileSyntax condSyntax, TokenData thenToken, ICompileSyntax thenSyntax,
                             ICompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        internal override Result Result(ContextBase context, Category category)
        {
            var trace = false;
            StartMethodDump(trace, context, category);
            return ReturnMethodDump(trace, InternalResult(context, category));
        }

        internal Result CondResult(ContextBase context, Category category)
        {
            return context
                .Result(category | Category.Type, Cond)
                .ConvertTo(TypeBase.Bit)
                .Align(context.AlignBits)
                .LocalBlock(category | Category.Type, context.RefAlignParam)
                .ConvertTo(TypeBase.Bit)
                ;
        }

        private Result ElseResult(ContextBase context, Category category)
        {
            if(Else == null)
                return TypeBase.Void.Result(category);
            return CondBranchResult(context, category, Else);
        }

        private Result ThenResult(ContextBase context, Category category) { return CondBranchResult(context, category, Then); }

        private Result CondBranchResult(ContextBase context, Category category, ICompileSyntax syntax)
        {
            var branchResult = context.Result(category | Category.Type, syntax).AutomaticDereference();
            if((category - Category.Type).IsNone)
                return branchResult.Align(context.RefAlignParam.AlignBits);

            var commonType = context.CommonType(this);
            var result = branchResult.Type
                .Conversion(category | Category.Type, commonType)
                .ReplaceArg(branchResult);
            return result.LocalBlock(category, context.RefAlignParam);
        }

        private Result InternalResult(ContextBase context, Category category)
        {
            var commonType = context.CommonType(this);
            if(category <= (Category.Type | Category.Size))
                return commonType.Result(category);

            var condResult = CondResult(context, category);
            return commonType.Result
                (
                    category,
                    () => condResult.Code.CreateThenElse(ThenResult(context, Category.Code).Code, ElseResult(context, Category.Code).Code),
                    () => condResult.Refs + context.CommonRefs(this)
                );
        }

        internal Result CommonResult(ContextBase context, Category category, bool thenIsPending, bool elseIsPending)
        {
            if(!thenIsPending)
                return ThenResult(context, category);
            if(!elseIsPending)
                return ElseResult(context, category);
            NotImplementedMethod(context, category, thenIsPending, elseIsPending);
            return null;
        }

        internal Result CommonResult(ContextBase context, Category category)
        {
            Tracer.Assert(category <= (Category.Type | Category.Refs));
            var thenResult = ThenResult(context, category);
            var elseResult = ElseResult(context, category);
            var result = new Result();
            if(category.HasType)
                result.Type = TypeBase.CommonType(thenResult.Type, elseResult.Type);
            if(category.HasRefs)
                result.Refs = thenResult.Refs + elseResult.Refs;
            return result;
        }

        internal override string DumpShort() { return "(" + Cond.DumpShort() + ")then(" + Then.DumpShort() + ")"; }
    }

    [Serializable]
    internal sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(ICompileSyntax condSyntax, TokenData thenToken, ICompileSyntax thenSyntax)
            : base(condSyntax, thenToken, thenSyntax, null) { }

        internal override ReniParser.ParsedSyntax CreateElseSyntax(TokenData token, ICompileSyntax elseSyntax) { return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax); }
    }

    [Serializable]
    internal sealed class ThenElseSyntax : CondSyntax
    {
        [Node]
        private readonly TokenData _elseToken;

        public ThenElseSyntax(ICompileSyntax condSyntax, TokenData thenToken, ICompileSyntax thenSyntax, TokenData elseToken,
                              ICompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax, elseSyntax) { _elseToken = elseToken; }

        internal override string DumpShort()
        {
            return base.DumpShort() + "else(" +
                   Else.DumpShort() + ")";
        }
    }
}