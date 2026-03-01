using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Feature;

static class Extension
{
    static readonly
        FunctionCache<Func<Category, ResultCache, ContextBase, ValueSyntax?, Result>, MetaFunction>
        MetaFunctionCache = new(function => new(function));

    static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>> ValueCache
        = new(function =>
            new(type => new(function, type)));

    static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Conversion>> ConversionCache
        = new(function =>
            new(type => new(function, type)));

    internal static Value Value(Func<Category, Result> function, TypeBase? target = null)
        => ValueCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

    internal static Conversion Conversion
        (Func<Category, Result> function, TypeBase? target = null)
        => ConversionCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

    internal static ObjectFunction FunctionFeature
    (
        Func<Category, IContextReference, TypeBase, Result> function,
        IContextReferenceProvider? target = null
    )
    {
        var context = (target ?? function.Target as IContextReferenceProvider).AssertNotNull();
        return new(function, context);
    }

    internal static Function FunctionFeature(Func<Category, TypeBase, Result> function) => new(function);

    internal static IImplementation FunctionFeature<T>
        (Func<Category, TypeBase, T, Result> function, T arg)
        => new ExtendedFunction<T>(function, arg);


    internal static MetaFunction MetaFeature
        (Func<Category, ResultCache, ContextBase, ValueSyntax?, Result> function)
        => MetaFunctionCache[function];

    static IEnumerable<TGeneric> CreateList<TGeneric>
        (IEnumerable<TGeneric>? baseList, Func<TGeneric> creator)
    {
        yield return creator();

        if(baseList == null)
            yield break;

        foreach(var item in baseList)
            yield return item;
    }

    extension(IImplementation feature)
    {
        internal Result GetResult
        (
            Category category,
            ResultCache left,
            SourcePart token,
            ContextBase context,
            ValueSyntax? right
        )
        {
            var metaFeature = ((IMetaImplementation)feature).Function;
            if(metaFeature != null)
                return metaFeature.GetResult(category, left, token, context, right);

            return feature
                .GetResult(category, token, context, right)
                .ReplaceArguments(left);
        }
    }

    extension(IConversion conversion)
    {
        internal TypeBase ResultType => conversion.GetResult(Category.Type).Type;

        internal Result GetResult(Category category)
        {
            var result = conversion.Execute(category);
            if(!result.HasIssue && category.HasCode && result.Code.ArgumentType != null)
                (result.Code.ArgumentType == conversion.Source).Assert
                    (result.DebuggerDump);

            return result;
        }
    }

    extension(IEvalImplementation feature)
    {
        internal Result GetResult
        (
            Category category,
            SourcePart currentTarget,
            ContextBase context,
            ValueSyntax? right
        )
        {
            (feature.Function == null || !feature.Function.IsImplicit || feature.Value == null)
                .Assert();

            var valueCategory = category;
            if(right != null)
                valueCategory = category | Category.Type;

            var valueResult = feature.ValueResult(right, valueCategory, context.RootContext);

            if(right == null)
                return valueResult
                    ?? new(category, IssueId.MissingRightExpression.GetIssue(context.RootContext, currentTarget, context));

            if(valueResult != null)
                return valueResult
                    .Type
                    .GetResult(category, valueResult, currentTarget, null, context, right);

            //Todo: Provide context information like this to "Expect"
            if(feature.Function == null)
                Dumpable.NotImplementedFunction(feature, category, currentTarget, context, right);
            (feature.Function != null).Expect();

            var argsResult = context.GetResultAsReferenceCache(right);
            var argsType = argsResult.Type;

            return feature
                .Function!
                .GetResult(category, argsType)
                .ReplaceArguments(argsResult);
        }

        Result? ValueResult
        (
            ValueSyntax? right,
            Category valueCategory,
            Root root
        )
        {
            if(feature.Function != null && feature.Function.IsImplicit)
                return feature
                    .Function
                    .GetResult(valueCategory, root.VoidType)
                    .ReplaceArguments(root.VoidType.GetResult(Category.All));

            if(right != null && feature.Function != null)
                return null;

            return feature.Value?.Execute(valueCategory);
        }
    }

    extension<T>(T target)
        where T : TypeBase
    {
        public IEnumerable<IGenericProviderForType> GetGenericProviders(IEnumerable<IGenericProviderForType>? baseList = null)
            => CreateList(baseList, () => new GenericProviderForType<T>(target));
    }

    extension<T>(IEnumerable<T?> enumerable)
        where T : class
    {
        internal T? DistinctNotNull()
            => enumerable
                .Where(x => x != null)
                .Distinct()
                .SingleOrDefault();
    }

    extension<T>(T target)
        where T : Definable
    {
        public IEnumerable<IDeclarationProvider> GenericListFromDefinable(IEnumerable<IDeclarationProvider>? baseList = null)
            => CreateList(baseList, () => new GenericProviderForDefinable<T>(target));
    }

    extension(Issue[] issues)
    {
        internal Result GetResult(Category category)
            => new(category, issues);
    }
}
