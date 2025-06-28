using System.Diagnostics;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;
using Reni.Validation;

namespace Reni;

sealed class ResultCache : DumpableObject
{
    internal interface IResultProvider
    {
        Result Execute(Category category);
    }

    sealed class ResultNotSupported : DumpableObject, IResultProvider
    {
        Result IResultProvider.Execute(Category category)
        {
            NotImplementedMethod(category);
            return default!;
        }
    }

    sealed class SimpleProvider : DumpableObject, IResultProvider
    {
        readonly Func<Category, Result> ObtainResult;

        public SimpleProvider(Func<Category, Result> obtainResult) => ObtainResult = obtainResult;

        Result IResultProvider.Execute(Category category) => ObtainResult(category);
    }

    [PublicAPI]
    sealed class CallStack : DumpableObject
    {
        internal readonly CallStack? Former;
        internal readonly Call Item;

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

        public CallStack(CallStack? former, Call item)
        {
            Former = former;
            Item = item;
        }
    }

    [PublicAPI]
    sealed class Call : DumpableObject
    {
        internal readonly Category Category;
        internal readonly ResultCache Item;

        public Call(Category category, ResultCache item)
        {
            Category = category;
            Item = item;
        }

        protected override string Dump(bool isRecursion)
        {
            var result = Item.NodeDump + " ";
            result += Category.Dump();
            if(Category != Item.PendingCategory)
                result += "(Pending=" + Item.PendingCategory.Dump() + ")";
            result += "\n";
            result += Tracer.Dump(Item.Provider);
            return result;
        }
    }

    [DisableDump]
    readonly bool InConstruction;

    [DisableDump]
    static CallStack? Current;

    [DisableDump]
    static readonly IResultProvider NotSupported = new ResultNotSupported();

    [DisableDump]
    static int NextObjectId;

    [DisableDump]
    internal readonly Result Data;

    [DisableDump]
    internal readonly IResultProvider Provider;

    [DisableDump]
    Category PendingCategory;

    [DisableDump]
    [PublicAPI]
    static Call[] Calls => Current?.ToEnumerable.ToArray() ?? [];

    [DisableDump]
    [DebuggerHidden]
    [DebuggerNonUserCode]
    internal TypeBase Type => Get(Category.Type).Type!;

    [DisableDump]
    [DebuggerHidden]
    [DebuggerNonUserCode]
    internal CodeBase Code => Get(Category.Code).Code!;

    [DisableDump]
    [DebuggerHidden]
    [DebuggerNonUserCode]
    internal Closures Closures => Get(Category.Closures).Closures!;

    [DisableDump]
    [DebuggerHidden]
    [DebuggerNonUserCode]
    [PublicAPI]
    internal Size Size => Get(Category.Size).Size!;

    [DisableDump]
    [DebuggerHidden]
    [DebuggerNonUserCode]
    [PublicAPI]
    internal bool? IsHollow => Get(Category.IsHollow).IsHollow;

    [DisableDump]
    [DebuggerHidden]
    [DebuggerNonUserCode]
    internal Issue[] Issues => Get(Category.IsHollow).Issues;

    internal ResultCache(IResultProvider obtainResult)
        : this(new(Category.None), obtainResult) { }


    ResultCache(Result data)
        : this(data, NotSupported) { }

    ResultCache(Func<Category, Result> obtainResult)
        : this(new(Category.None), new SimpleProvider(obtainResult)) { }

    ResultCache(Result data, IResultProvider provider)
        : base(NextObjectId++)
    {
        InConstruction = true;
        Data = data;
        Provider = provider;

        InConstruction = false;
        StopByObjectIds();
    }

    public override string DumpData()
    {
        var result = "";
        if(PendingCategory != Category.None)
            result += $"PendingCategory={PendingCategory.Dump()}\n";
        result += Data.DumpData();
        return result;
    }

    public static ResultCache CreateInstance(Func<Category, Result> obtainResult)
        => new(obtainResult);

    public static implicit operator ResultCache(Result x) => new(x);

    /// <summary>
    ///     Try to update all categories according to <see cref="category" />.
    ///     Complete categories left untouched.
    ///     Pending categories are treated by recursion handlers
    /// </summary>
    /// <param name="category"></param>
    [DebuggerHidden]
    void Update(Category category)
    {
        var availableCategory = category.Without(PendingCategory);

        SimpleUpdate(availableCategory.Without(Data.CompleteCategory));
        // Watch out! Data.CompleteCategory may have changed by SimpleUpdate

        // Getting type first is required to treat recursivity
        if(availableCategory.Without(Data.CompleteCategory).HasType())
            LinearUpdate(Category.Type);
        LinearUpdate(availableCategory.Without(Data.CompleteCategory));
        RecursiveUpdate(category & PendingCategory);
    }

    /// <summary>
    ///     Try to update simple cases that are provider independent.
    ///     For instance <see cref="IsHollow" /> may be obvious if size has been obtained already.
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
    ///     Pending categories are not treated here
    ///     Since it may cause recursion, pending categories are temporarily extended during call.
    /// </summary>
    /// <param name="category"></param>
    void LinearUpdate(Category category)
    {
        if(category == Category.None)
            return;

        var oldPendingCategory = PendingCategory;
        PendingCategory |= category;

        try
        {
            var result = Provider.Execute(category);
            result.IsValidOrIssue(category).Assert();

            Data.Update(result);
            PendingCategory = PendingCategory.Without(result.CompleteCategory);
        }
        finally
        {
            PendingCategory = oldPendingCategory.Without(Data.CompleteCategory);
        }
    }

    /// <summary>
    ///     Try to update categories that are already pending.
    ///     It uses recursion handlers that must not be recursive.
    /// </summary>
    /// <param name="category"></param>
    void RecursiveUpdate(Category category)
    {
        if(InConstruction || ObjectId == 3)
            return;
        if(category == Category.None)
            return;

        if(category == Category.Closures)
        {
            Data.Closures = Closures.GetRecursivity();
            return;
        }

        NotImplementedMethod(category);
        return;
    }

    public static Result operator &(ResultCache resultCache, Category category)
        =>  resultCache.Get(category);

    bool InGet;
    /// <summary>
    ///     Obtain the categories requested.
    ///     This will also try to obtain categories that are not yet obtained.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [DebuggerHidden]
    internal Result Get(Category category)
    {
        var inGet = InGet;
        InGet = true;

        var trace = ObjectId.In() ;
        StartMethodDump(trace, category, "inGet", inGet);
        try
        {
            BreakExecution();
            Update(category);
            Dump(nameof(Provider), Provider);
            var t = Data.Type;
            var completeCategory = Data.CompleteCategory;
if(ObjectId != 3)            completeCategory
                .Contains(category)
                .Assert(() => $"{ObjectId}i: {Data.Dump()}\nPendingCategory={PendingCategory.Dump()}");
            return ReturnMethodDump(Data & category);
        }
        finally
        {
            EndMethodDump();
            InGet = inGet;
        }
    }

    /// <summary>
    ///     Obtain the categories requested.
    ///     Internal function that collects some information for debugging purposes
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [PublicAPI]
    void GuardedUpdate(Category category)
    {
        try
        {
            Current = new(Current, new(category, this));

            Update(category);
        }
        finally
        {
            Current = Current.AssertNotNull().Former;
        }
    }

    internal TypedData Evaluate(Root root) => Get(Category.All).GetValue(root.ExecutionContext);
}
