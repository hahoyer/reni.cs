using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Feature;

static class Extension
{
    static readonly
        FunctionCache<Func<Category, ResultCache, ContextBase, ValueSyntax, Result>, MetaFunction>
        MetaFunctionCache = new(function => new(function));

    static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>> ValueCache
        = new(
            function =>
                new(type => new(function, type)));

    static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Conversion>> ConversionCache
        = new(
            function =>
                new(type => new(function, type)));

    internal static Value Value(Func<Category, Result> function, TypeBase target = null)
        => ValueCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

    internal static Conversion Conversion
        (Func<Category, Result> function, TypeBase target = null)
        => ConversionCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

    internal static ObjectFunction FunctionFeature
    (
        Func<Category, IContextReference, TypeBase, Result> function,
        IContextReferenceProvider target = null
    )
    {
        var context = (target ?? function.Target as IContextReferenceProvider).AssertNotNull();
        return new(function, context);
    }

    internal static Function FunctionFeature(Func<Category, TypeBase, Result> function) => new(function);

    internal static IImplementation FunctionFeature<T>
        (Func<Category, TypeBase, T, Result> function, T arg)
        => new ExtendedFunction<T>(function, arg);


    internal static IValue ExtendedValue(this IImplementation feature)
    {
        var function = ((IEvalImplementation)feature).Function;
        if(function != null && function.IsImplicit)
            return null;

        return feature.Value;
    }

    internal static MetaFunction MetaFeature
        (Func<Category, ResultCache, ContextBase, ValueSyntax, Result> function)
        => MetaFunctionCache[function];

    internal static TypeBase ResultType(this IConversion conversion)
        => conversion.GetResult(Category.Type).Type;

    internal static Result GetResult(this IConversion conversion, Category category)
    {
        var result = conversion.Execute(category);
        (result != null).Assert();

        if(result.HasIssue != true && category.HasCode() && result.Code.ArgumentType != null)
            (result.Code.ArgumentType == conversion.Source).Assert
                (() => result.DebuggerDump());

        return result;
    }

    public static IEnumerable<IGenericProviderForType> GenericListFromType<T>
        (this T target, IEnumerable<IGenericProviderForType> baseList = null)
        where T : TypeBase
        => CreateList(baseList, () => new GenericProviderForType<T>(target));

    public static IEnumerable<IDeclarationProvider> GenericListFromDefinable<T>
        (this T target, IEnumerable<IDeclarationProvider> baseList = null)
        where T : Definable
        => CreateList(baseList, () => new GenericProviderForDefinable<T>(target));

    static IEnumerable<TGeneric> CreateList<TGeneric>
        (IEnumerable<TGeneric> baseList, Func<TGeneric> creator)
    {
        yield return creator();

        if(baseList == null)
            yield break;

        foreach(var item in baseList)
            yield return item;
    }

    internal static Result GetResult
    (
        this IEvalImplementation feature,
        Category category,
        SourcePart currentTarget,
        ContextBase context,
        ValueSyntax right
    )
    {
        (feature.Function == null || !feature.Function.IsImplicit || feature.Value == null)
            .Assert();

        var valueCategory = category;
        if(right != null)
            valueCategory = category | Category.Type;

        var valueResult = feature.ValueResult(context, right, valueCategory);

        if(right == null)
        {
            if(valueResult != null)
                return valueResult;

            return IssueId
                .MissingRightExpression
                .GetResult(category, currentTarget, context);
        }

        if(valueResult == null)
        {
            if(feature.Function == null)
                Dumpable.NotImplementedFunction(feature, category, currentTarget, context, right);

            (feature.Function != null).Assert();

            var argsResult = context.GetResultAsReferenceCache(right);
            var argsType = argsResult.Type;
            if(argsType == null)
                return argsResult.Get(category);

            return feature
                .Function
                .GetResult(category, argsType)
                .ReplaceArguments(argsResult);
        }

        var (result, found) = valueResult
            .Type
            .GetResult(category, valueResult, currentTarget, definable: null, context, right);
        right.Anchor.Items.Single(item=>item.TokenClass is LeftParenthesis).Semantic.Declaration[context]= found;
        return result;
    }

    static Result ValueResult
    (
        this IEvalImplementation feature,
        ContextBase context,
        ValueSyntax right,
        Category valueCategory
    )
    {
        if(feature.Function != null && feature.Function.IsImplicit)
            return feature
                .Function
                .GetResult(valueCategory, Root.VoidType)
                .ReplaceArguments(Root.VoidType.GetResult(Category.All));

        if(right != null && feature.Function != null)
            return null;

        return feature.Value?.Execute(valueCategory);
    }

    internal static Result GetResult
    (
        this IImplementation feature,
        Category category,
        ResultCache left,
        SourcePart token,
        ContextBase context,
        ValueSyntax right
    )
    {
        var metaFeature = ((IMetaImplementation)feature).Function;
        if(metaFeature != null)
            return metaFeature.GetResult(category, left, context, right);

        return feature
            .GetResult(category, token, context, right)
            .ReplaceArguments(left);
    }

    internal static T DistinctNotNull<T>(this IEnumerable<T> enumerable)
        where T : class
        => enumerable
            .Where(x => x != null)
            .Distinct()
            .SingleOrDefault();

    internal static Result GetResult(this Issue[] issues, Category category, Root root) => new(category, issues);
}