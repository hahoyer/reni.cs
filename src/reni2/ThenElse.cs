using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni
{
    sealed class CondSyntax : CompileSyntax, IRecursionHandler
    {
        [Node]
        [EnableDump]
        readonly CompileSyntax Cond;

        [Node]
        [EnableDump]
        readonly CompileSyntax Then;

        [Node]
        [EnableDump]
        readonly CompileSyntax Else;

        internal CondSyntax
            (
            CompileSyntax condSyntax,
            CompileSyntax thenSyntax,
            CompileSyntax elseSyntax = null)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        CondSyntax(CondSyntax other, CompileSyntax elseSyntax)
        {
            Cond = other.Cond;
            Then = other.Then;
            Tracer.Assert(other.Else == null);
            Else = elseSyntax;
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return Cond;
                yield return Then;
                yield return Else;
            }
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => InternalResult(context, category);

        internal override IRecursionHandler RecursionHandler => this;

        internal override ResultCache.IResultProvider FindSource
            (IContextReference ext, ContextBase context)
        {
            var result = DirectChildren
                .Cast<CompileSyntax>()
                .SelectMany(item => item.ResultCache)
                .Where(item => item.Value.Exts.Contains(ext))
                .Where(item => item.Key == context)
                ;

            return result.FirstOrDefault().Value?.Provider;
        }

        Result CondResult(ContextBase context, Category category)
            => context.Result(category.Typed, Cond)
                .Conversion(context.RootContext.BitType.Align)
                .LocalBlock(category.Typed)
                .Conversion(context.RootContext.BitType);

        Result ElseResult(ContextBase context, Category category)
        {
            if(Else == null)
                return context
                    .RootContext.VoidType.Result(category);
            return BranchResult(context, category, Else);
        }

        Result ThenResult(ContextBase context, Category category)
            => BranchResult(context, category, Then);

        Result BranchResult(ContextBase context, Category category, CompileSyntax syntax)
        {
            var result = context.Result(category.Typed, syntax);
            if(result == null)
                return null;

            var branchResult = result.AutomaticDereferenceResult;

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
            Tracer.ConditionalBreak(elseResult == null);
            return commonType
                .Result
                (
                    category,
                    () => condResult.Code.ThenElse(thenResult.Code, elseResult.Code),
                    () => condResult.Exts + thenResult.Exts + elseResult.Exts
                );
        }

        TypeBase CommonType(ContextBase context)
        {
            if(Else == null)
                return context
                    .RootContext.VoidType;
            var thenType = Then.Type(context)?.AutomaticDereferenceType;
            var elseType = Else.Type(context)?.AutomaticDereferenceType;
            if(thenType == null)
                return elseType?.Align;
            if(elseType == null)
                return thenType.Align;
            return thenType.CommonType(elseType).Align;
        }

        internal override Syntax CreateElseSyntax(CompileSyntax elseSyntax)
        {
            Tracer.Assert(Else == null);
            return new CondSyntax(this, elseSyntax);
        }

        Result IRecursionHandler.Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            CompileSyntax syntax,
            bool asReference)
        {
            Tracer.Assert(syntax == this);

            if(!asReference && (category | pendingCategory) <= Category.Type && Else == null)
                return context.RootContext.VoidType.Result(Category.Type);

            NotImplementedMethod(context, category, pendingCategory, syntax, asReference);
            return null;
        }
    }
}