using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Type;
using Reni.Validation;

namespace Reni;

sealed class ResultCache : DumpableObject
{
    internal interface IResultProvider
    {
        Result Execute(Category category);
    }

    internal interface IRecursiveResultProvider
    {
        Result Execute(Category category);
    }

    sealed class ResultNotSupported : DumpableObject, IResultProvider
    {
        Result IResultProvider.Execute(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }

    sealed class SimpleProvider : DumpableObject, IResultProvider
    {
        readonly Func<Category, Result> ObtainResult;

        public SimpleProvider(Func<Category, Result> obtainResult) => ObtainResult = obtainResult;

        Result IResultProvider.Execute(Category category) => ObtainResult(category);
    }

    sealed class CallStack : DumpableObject
    {
        internal CallStack Former;
        internal Call Item;

        internal IEnumerable<Call> ToEnumerable
        {
            get
            {
                yield return Item;
                if(Former != null)
                    foreach(var call in Former.ToEnumerable)
                        yield return call;
            }
        }
    }

    sealed class Call : DumpableObject
    {
        internal Category Category;
        internal ResultCache Item;

        protected override string Dump(bool isRecursion)
        {
            var result = Item.NodeDump + " ";
            result += Category.Dump();
            if(Category != Item.Data.PendingCategory)
                result += "(Pending=" + Item.Data.PendingCategory.Dump() + ")";
            result += "\n";
            result += Tracer.Dump(Item.Provider);
            return result;
        }
    }

    [DisableDump]
    const string FunctionDump = "";

    [DisableDump]
    static CallStack Current;

    [DisableDump]
    static readonly IResultProvider NotSupported = new ResultNotSupported();

    [DisableDump]
    static int NextObjectId;

    [DisableDump]
    internal Result Data { get; }

    [DisableDump]
    internal IResultProvider Provider { get; }

    [DisableDump]
    [PublicAPI]
    static Call[] Calls => Current?.ToEnumerable.ToArray() ?? new Call[0];

    [DisableDump]
    internal TypeBase Type => Get(Category.Type).Type;

    [DisableDump]
    internal CodeBase Code => Get(Category.Code).Code;

    [DisableDump]
    internal Closures Closures => Get(Category.Closures).Closures;

    [DisableDump]
    internal Size Size => Get(Category.Size).Size;

    [DisableDump]
    internal bool? IsHollow => Get(Category.IsHollow).IsHollow;

    [DisableDump]
    internal Issue[] Issues => Get(Category.IsHollow).Issues;

    internal ResultCache(IResultProvider obtainResult)
        : this()
    {
        Data = new(Category.None);
        Provider = obtainResult ?? NotSupported;
    }

    internal ResultCache(Func<Category, Result> obtainResult)
        : this()
    {
        Data = new(Category.None);
        Provider = new SimpleProvider(obtainResult);
    }

    ResultCache()
        : base(NextObjectId++)
        => StopByObjectIds();

    ResultCache(Result data)
        : this()
    {
        Data = data;
        Provider = NotSupported;
    }

    public override string DumpData()
    {
        var result = FunctionDump;
        if(result != "")
            result += "\n";
        result += Data.DumpData();
        return result;
    }

    public static implicit operator ResultCache(Result x) => new(x);

    /// <summary>
    ///     Try to update all categories according to <see cref="category" />.
    ///     Complete categories left untouched.
    ///     Pending categories are treated by recursion handlers
    /// </summary>
    /// <param name="category"></param>
    //[DebuggerHidden]
    void Update(Category category)
    {
        var availableCategory = category.Without(Data.PendingCategory);
        var pendingCategory = category & Data.PendingCategory;

        SimpleUpdate(availableCategory.Without(Data.CompleteCategory));
        // Watch out! Data.CompleteCategory may have changed by SimpleUpdate
        SmartUpdate(availableCategory.Without(Data.CompleteCategory) | pendingCategory, pendingCategory == Category.None);
    }

    /// <summary>
    ///     Try to update simple cases that provider independent.
    ///     For instance <see cref="IsHollow" /> may be obvious since it is a function.
    ///     This is essential to avoid recursion.
    /// </summary>
    /// <param name="category"></param>
    void SimpleUpdate(Category category)
    {
        if(category.HasIsHollow())
            Data.IsHollow = Data.FindIsHollow;

        if(category.HasSize())
            Data.Size = Data.FindSize;

        if(category.HasClosures())
            Data.Closures = Data.FindClosures;
    }

    /// <summary>
    ///     Update anything that is hasn't been obtained yet.
    ///     Since it may cause recursion, pending categories are temporarily extended during call.
    ///     It executes in two variants: linear or recursive mode.
    /// </summary>
    /// <param name="category"></param>
    /// <param name="isLinear"></param>
    void SmartUpdate(Category category, bool isLinear)
    {
        if(category == Category.None)
            return;
        var oldPendingCategory = Data.PendingCategory;
        Data.HasIssue.ConditionalBreak();
        Data.PendingCategory |= category;

        try
        {
            var result = isLinear
                ? Provider.Execute(category)
                : ((IRecursiveResultProvider)Provider).Execute(category);

            (result != null).Assert(() => Tracer.Dump(Provider));
            result.IsValidOrIssue(category).Assert();
            Data.Update(result);
        }
        finally
        {
            Data.PendingCategory = oldPendingCategory.Without(Data.CompleteCategory);
        }
    }

    public static Result operator &(ResultCache resultCache, Category category)
        => resultCache.Get(category);

    /// <summary>
    ///     Obtain the categories requested.
    ///     This will try to obtain categories that are not yet obtained.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    //[DebuggerHidden]
    internal Result Get(Category category)
    {
        var trace = ObjectId.In();
        StartMethodDump(trace, category);
        try
        {
            BreakExecution();
            Update(category);
            Dump(nameof(Provider), Provider);
            Data.CompleteCategory.Contains(category).Assert(Data.Dump);
            return ReturnMethodDump(Data & category);
        }
        finally
        {
            EndMethodDump();
        }
    }

    /// <summary>
    ///     Obtain the categories requested.
    /// Internal function that collects some information for debugging purposes
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [PublicAPI]
    void GuardedUpdate(Category category)
    {
        try
        {
            Current = new()
            {
                Former = Current, Item = new()
                {
                    Category = category, Item = this
                }
            };

            Update(category);
        }
        finally
        {
            Current = Current.Former;
        }
    }
}