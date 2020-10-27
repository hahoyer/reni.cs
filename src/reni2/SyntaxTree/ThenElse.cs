using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.SyntaxTree
{
    sealed class CondSyntax : ValueSyntax, IRecursionHandler
    {
        [Node]
        [EnableDump]
        readonly ValueSyntax Cond;

        [Node]
        [EnableDump]
        readonly ValueSyntax Else;

        [Node]
        [EnableDump]
        readonly ValueSyntax Then;

        internal CondSyntax
            (ValueSyntax condSyntax, ValueSyntax thenSyntax, ValueSyntax elseSyntax, Anchor anchor)
            : base(anchor)
        {
            Cond = condSyntax;
            Then = thenSyntax;
            Else = elseSyntax;
        }

        protected override int LeftDirectChildCountInternal => Else == null? 1 : 2;

        protected override int DirectChildCount => 3;

        internal override IRecursionHandler RecursionHandler => this;

        Result IRecursionHandler.Execute
        (
            ContextBase context,
            Category category,
            Category pendingCategory,
            ValueSyntax syntax,
            bool asReference
        )
        {
            (syntax == this).Assert();

            if(!asReference && (category | pendingCategory) <= Category.Type && Else == null)
                return context.RootContext.VoidType.Result(Category.Type);

            NotImplementedMethod(context, category, pendingCategory, syntax, asReference);
            return null;
        }

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Cond, 1 => Then, 2 => Else, _ => null
            };

        internal override Result ResultForCache(ContextBase context, Category category)
            => InternalResult(context, category);

        Result CondResult(ContextBase context, Category category)
            => context.Result(category.WithType, Cond)
                .Conversion(context.RootContext.BitType.Align)
                .LocalBlock(category.WithType)
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
            var result = context.Result(category.WithType, syntax);
            if(result == null)
                return null;

            var branchResult = result;
            if(branchResult.HasIssue)
                return branchResult;

            var commonType = CommonType(context);
            return branchResult.Type
                       .Conversion(category.WithType, commonType)
                       .ReplaceArg(branchResult) &
                   category;
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
                    () => condResult.Closures + thenResult.Closures + elseResult.Closures
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
    }
}