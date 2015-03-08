using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Type;

namespace Reni
{
    sealed class CondSyntax : CompileSyntax
    {
        [Node]
        readonly CompileSyntax Cond;

        [Node]
        readonly CompileSyntax Then;

        [Node]
        readonly CompileSyntax NativeElse;

        readonly CompileSyntax Else;

        internal CondSyntax
            (
            CompileSyntax condSyntax,
            Token thenToken,
            CompileSyntax thenSyntax,
            CompileSyntax elseSyntax = null,
            SourcePart sourcePart = null)
            : base(thenToken, sourcePart)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            NativeElse = elseSyntax;
            Else = elseSyntax ?? new EmptyList(thenToken);
        }

        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new CondSyntax(Cond, Token, Then, NativeElse, sourcePart);

        internal override Result ResultForCache(ContextBase context, Category category)
            => InternalResult(context, category);

        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {Cond, Then, NativeElse};

        Result CondResult(ContextBase context, Category category) => Cond
            .Result(context, category.Typed)
            .Conversion(context.RootContext.BitType.Align)
            .LocalBlock(category.Typed)
            .Conversion(context.RootContext.BitType);

        Result ElseResult(ContextBase context, Category category)
            => BranchResult(context, category, Else);
        Result ThenResult(ContextBase context, Category category)
            => BranchResult(context, category, Then);

        Result BranchResult(ContextBase context, Category category, CompileSyntax syntax)
        {
            var branchResult = syntax
                .Result(context, category.Typed).AutomaticDereferenceResult;

            var commonType = CommonType(context);
            return branchResult.Type
                .Conversion(category.Typed, commonType)
                .ReplaceArg(branchResult)
                .LocalBlock(category.Typed)
                .Conversion(commonType)
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
                    category,
                    () => condResult.Code.ThenElse(thenResult.Code, elseResult.Code),
                    () => condResult.Exts + thenResult.Exts + elseResult.Exts
                );
        }

        TypeBase CommonType(ContextBase context) => Then
            .Type(context)
            .CommonType(Else.Type(context))
            .Align;

        internal override Syntax CreateElseSyntax(Token token, CompileSyntax elseSyntax)
        {
            Tracer.Assert(NativeElse == null);
            return new CondSyntax(Cond, Token, Then, elseSyntax, token.SourcePart);
        }

        internal override Result PendingResultForCache(ContextBase context, Category category)
        {
            if(NativeElse == null)
                return context
                    .RootContext.VoidType.Result(category);
            return base.PendingResultForCache(context, category);
        }
    }
}