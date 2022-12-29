using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        Result Execute(Category category, Category pendingCategory);
    }

    sealed class ResultNotSupported : DumpableObject, IResultProvider
    {
        Result IResultProvider.Execute(Category category, Category pendingCategory)
        {
            NotImplementedMethod(category, pendingCategory);
            return null;
        }
    }

    sealed class SimpleProvider : DumpableObject, IResultProvider
    {
        readonly Func<Category, Result> ObtainResult;

        public SimpleProvider(Func<Category, Result> obtainResult) => ObtainResult = obtainResult;

        Result IResultProvider.Execute(Category category, Category pendingCategory)
        {
            pendingCategory.IsNone.Assert();
            return ObtainResult(category);
        }
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
    static CallStack Current;

    [DisableDump]
    static readonly IResultProvider NotSupported = new ResultNotSupported();

    [DisableDump]
    static int NextObjectId;

    [DisableDump]
    internal Result Data { get; } = new(Category.None);

    [DisableDump]
    internal IResultProvider Provider { get; }

    [DisableDump]
    internal string FunctionDump = "";

    [DisableDump]
    [PublicAPI]
    static Call[] Calls => Current?.ToEnumerable.ToArray() ?? new Call[0];

    [DisableDump]
    internal TypeBase Type => GetCategories(Category.Type).Type;

    [DisableDump]
    internal CodeBase Code => GetCategories(Category.Code).Code;

    [DisableDump]
    internal Closures Closures => GetCategories(Category.Closures).Closures;

    [DisableDump]
    internal Size Size => GetCategories(Category.Size).Size;

    [DisableDump]
    internal bool? IsHollow => GetCategories(Category.IsHollow).IsHollow;

    [DisableDump]
    internal Issue[] Issues => GetCategories(Category.IsHollow).Issues;

    internal ResultCache(IResultProvider obtainResult)
        : this() => Provider = obtainResult ?? NotSupported;

    internal ResultCache(Func<Category, Result> obtainResult)
        : this() => Provider = new SimpleProvider(obtainResult);

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

    //[DebuggerHidden]
    void Update(Category category)
    {
        if(Data.HasIssue)
            return;

        var localCategory = category.Without(Data.CompleteCategory | Data.PendingCategory);

        if(localCategory.HasIsHollow() && Data.FindIsHollow != null)
        {
            Data.IsHollow = Data.FindIsHollow;
            localCategory = localCategory.Without(Category.IsHollow);
        }

        if(localCategory.HasSize() && Data.FindSize != null)
        {
            Data.Size = Data.FindSize;
            localCategory = localCategory.Without(Category.Size);
        }

        if(localCategory.HasClosures() && Data.FindClosures != null)
        {
            Data.Closures = Data.FindClosures;
            localCategory = localCategory.Without(Category.Closures);
        }

        if(!localCategory.HasAny && !(category & Data.PendingCategory).HasAny)
            return;

        var oldPendingCategory = Data.PendingCategory;
        try
        {
            Data.PendingCategory |= localCategory;
            var result = Provider.Execute(localCategory, oldPendingCategory & category);

            (result != null).Assert(() => Tracer.Dump(Provider));
            result.IsValidOrIssue(localCategory).Assert();
            Data.Update(result);
        }
        finally
        {
            Data.PendingCategory = oldPendingCategory.Without(Data.CompleteCategory);
        }
    }

    public static Result operator &(ResultCache resultCache, Category category)
        => resultCache.GetCategories(category);

    //[DebuggerHidden]
    internal Result GetCategories(Category category)
    {
        var trace = ObjectId.In();
        StartMethodDump(trace, category);
        try
        {
            BreakExecution();
            GuardedUpdate(category);
            Dump(nameof(Provider), Provider);
            return ReturnMethodDump(Data & category);
        }
        finally
        {
            EndMethodDump();
        }
    }

    [DebuggerHidden]
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