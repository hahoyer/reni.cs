using System.Collections.Generic;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    sealed class CondSyntax : ValueSyntax, IRecursionHandler
    {
        [Node]
        [EnableDump]
        readonly ValueSyntax Cond;

        [Node]
        [EnableDump]
        readonly ValueSyntax Then;

        [Node]
        [EnableDump]
        readonly ValueSyntax Else;

        internal static Result<ValueSyntax> Create
            (BinaryTree condition, BinaryTree thenBinaryTree, BinaryTree elseBinaryTree, BinaryTree binaryTree, ISyntaxScope scope)
        {
            var conditionValue = condition.Syntax(scope);
            var thenValue = thenBinaryTree?.Syntax(scope);
            var elseValue = elseBinaryTree?.Syntax(scope);
            var result = new CondSyntax
                (conditionValue.Target, thenValue?.Target, elseValue?.Target, binaryTree);
            var issues = conditionValue.Issues.plus(thenValue?.Issues).plus(elseValue?.Issues);
            return new Result<ValueSyntax>(result, issues);
        }

        CondSyntax(ValueSyntax condSyntax, ValueSyntax thenSyntax, ValueSyntax elseSyntax, BinaryTree target)
            : base(target)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        protected override IEnumerable<Syntax> GetChildren() => T(Cond, Then,Else);

        internal override Result ResultForCache(ContextBase context, Category category)
            => InternalResult(context, category);

        internal override IRecursionHandler RecursionHandler => this;

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

        Result BranchResult(ContextBase context, Category category, ValueSyntax syntax)
        {
            var result = context.Result(category.Typed, syntax);
            if(result == null)
                return null;

            var branchResult = result;
            if(branchResult.HasIssue)
                return branchResult;

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
            if(condResult.HasIssue || thenResult.HasIssue || elseResult.HasIssue)
                return condResult + thenResult + elseResult;

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
            ValueSyntax syntax,
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