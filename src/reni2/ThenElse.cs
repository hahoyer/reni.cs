using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    sealed class CondSyntax : Value, IRecursionHandler
    {
        [Node]
        [EnableDump]
        readonly Value Cond;

        [DisableDump]
        internal override SourcePart Token { get; }

        [Node]
        [EnableDump]
        readonly Value Then;

        [Node]
        [EnableDump]
        readonly Value Else;

        internal static Result<Value> Create
            (Syntax condition, SourcePart token, Syntax thenSyntax, Syntax elseSyntax)
        {
            var conditionValue = condition.Value;
            var thenValue = thenSyntax?.Value;
            var elseValue = elseSyntax?.Value;
            var result = new CondSyntax
                (conditionValue.Target, token, thenValue?.Target, elseValue?.Target);
            var issues = conditionValue.Issues.plus(thenValue?.Issues).plus(elseValue?.Issues);
            return new Result<Value>(result, issues);
        }

        CondSyntax(Value condSyntax, SourcePart token, Value thenSyntax, Value elseSyntax)
        {
            Cond = condSyntax;
            Token = token;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren
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

        internal override SourcePosn SourceStart => Cond.SourceStart;
        internal override SourcePosn SourceEnd => Else?.SourceEnd ?? Then?.SourceEnd ?? Token.End;

        internal override ResultCache.IResultProvider FindSource
            (IContextReference ext, ContextBase context)
        {
            var result = DirectChildren
                .Cast<Value>()
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

        Result BranchResult(ContextBase context, Category category, Value syntax)
        {
            var result = context.Result(category.Typed, syntax);
            if(result == null)
                return null;

            var branchResult = result;

            var commonType = CommonType(context);
            return branchResult.Type
                .Conversion(category.Typed, commonType)
                .ReplaceArg(branchResult)
                & category;
        }

        Result InternalResult(ContextBase context, Category category)
        {
            var commonType = CommonType(context);
            if(category <= Category.Type.Replenished)
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

        TypeBase CommonType(ContextBase context)
        {
            if(Else == null)
                return context
                    .RootContext.VoidType;

            var thenType = Then.Type(context);
            var elseType = Else.Type(context);
            if(thenType == null)
                return elseType?.Align;
            if(elseType == null)
                return thenType.Align;

            return thenType.CommonType(elseType).Align;
        }

        Result IRecursionHandler.Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            Value syntax,
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