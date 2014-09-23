using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    abstract class CondSyntax : CompileSyntax
    {
        [Node]
        internal readonly CompileSyntax Cond;

        [Node]
        internal readonly CompileSyntax Then;

        [NotNull]
        [Node]
        internal readonly CompileSyntax Else;

        protected CondSyntax
            (
            CompileSyntax condSyntax,
            TokenData thenToken,
            CompileSyntax thenSyntax,
            CompileSyntax elseSyntax)
            : base(thenToken)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax ?? new EmptyList(thenToken, thenToken);
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return InternalResult(context, category);
        }

        [DisableDump]
        protected override ParsedSyntaxBase[] Children { get { return new ParsedSyntaxBase[] {Cond, Then, Else}; } }

        Result CondResult(ContextBase context, Category category)
        {
            return Cond
                .Result(context, category.Typed)
                .Conversion(context.RootContext.BitType)
                .LocalBlock(category.Typed)
                .ObviousExactConversion(context.RootContext.BitType);
        }

        Result ElseResult(ContextBase context, Category category) { return BranchResult(context, category, Else); }
        Result ThenResult(ContextBase context, Category category) { return BranchResult(context, category, Then); }

        Result BranchResult(ContextBase context, Category category, CompileSyntax syntax)
        {
            var branchResult = syntax
                .Result(context, category.Typed).AutomaticDereferenceResult;

            var commonType = CommonType(context);
            return branchResult.Type
                .Conversion(category.Typed, commonType)
                .ReplaceArg(branchResult)
                .LocalBlock(category.Typed)
                .ObviousExactConversion(commonType)
                & category;
        }

        Result InternalResult(ContextBase context, Category category)
        {
            var commonType = CommonType(context);
            if(category <= (Category.Type.Replenished))
                return commonType.Result(category);

            var branchCategory = category & Category.Code.Replenished;
            var condResult = CondResult(context, category);
            var thenResult = ThenResult(context, branchCategory);
            var elseResult = ElseResult(context, branchCategory);
            return commonType
                .Result
                (
                    category
                    ,
                    () => condResult.Code.ThenElse(thenResult.Code, elseResult.Code)
                    ,
                    () => condResult.Exts + thenResult.Exts + elseResult.Exts
                );
        }

        TypeBase CommonType(ContextBase context)
        {
            return Then
                .Type(context)
                .CommonType(Else.Type(context))
                .UniqueAlign;
        }

        protected override string GetNodeDump() { return "(" + Cond.NodeDump + ")then(" + Then.NodeDump + ")"; }
    }

    sealed class ThenSyntax : CondSyntax
    {
        internal ThenSyntax(CompileSyntax condSyntax, TokenData thenToken, CompileSyntax thenSyntax)
            : base(condSyntax, thenToken, thenSyntax, null)
        {}

        internal override ParsedSyntax CreateElseSyntax(TokenData token, CompileSyntax elseSyntax)
        {
            return new ThenElseSyntax(Cond, Token, Then, token, elseSyntax);
        }

        internal override Result ObtainPendingResult(ContextBase context, Category category)
        {
            return context
                .RootContext
                .VoidResult(category);
        }
    }

    sealed class ThenElseSyntax : CondSyntax
    {
        [Node]
        readonly TokenData _elseToken;

        public ThenElseSyntax
            (
            CompileSyntax condSyntax,
            TokenData thenToken,
            CompileSyntax thenSyntax,
            TokenData elseToken,
            CompileSyntax elseSyntax)
            : base(condSyntax, thenToken, thenSyntax, elseSyntax)
        {
            _elseToken = elseToken;
        }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump() + "else(" +
                Else.NodeDump + ")";
        }
    }
}