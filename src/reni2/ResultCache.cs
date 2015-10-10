using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni
{
    sealed class ResultCache : DumpableObject, ITreeNodeSupport
    {
        internal interface IResultProvider
        {
            Result Execute(Category category, Category pendingCategory);
            object Target { get; }
        }

        sealed class ResultNotSupported : DumpableObject, IResultProvider
        {
            Result IResultProvider.Execute(Category category, Category pendingCategory)
            {
                NotImplementedMethod(category, pendingCategory);
                return null;
            }

            object IResultProvider.Target => null;
        }

        sealed class SimpleProvider : DumpableObject, IResultProvider
        {
            readonly Func<Category, Result> ObtainResult;

            public SimpleProvider(Func<Category, Result> obtainResult)
            {
                ObtainResult = obtainResult;
            }

            Result IResultProvider.Execute(Category category, Category pendingCategory)
            {
                Tracer.Assert(pendingCategory.IsNone);
                return ObtainResult(category);
            }

            object IResultProvider.Target => null;
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
            internal ResultCache Item;
            internal Category Category;

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
        IResultProvider Provider { get; }
        [DisableDump]
        internal string FunctionDump = "";
        [DisableDump]
        internal Syntax Syntax => (Provider.Target as ContextBase.ResultProvider)?.Syntax;

        [DisableDump]
        static Call[] Calls => Current?.ToEnumerable.ToArray() ?? new Call[0];

        [DisableDump]
        internal Result Data { get; } = new Result();

        internal ResultCache(IResultProvider obtainResult)
            : base(NextObjectId++)
        {
            Provider = obtainResult ?? NotSupported;
        }

        internal ResultCache(Func<Category, Result> obtainResult)
            : base(NextObjectId++)
        {
            Provider = new SimpleProvider(obtainResult);
        }

        ResultCache(Result data)
            : base(NextObjectId++)
        {
            Data = data;
            Provider = NotSupported;
        }

        public static implicit operator ResultCache(Result x) => new ResultCache(x);

        //[DebuggerHidden]
        void Update(Category category)
        {
            var localCategory = category - Data.CompleteCategory - Data.PendingCategory;

            if(localCategory.HasHllw && Data.FindHllw != null)
            {
                Data.Hllw = Data.FindHllw;
                localCategory -= Category.Hllw;
            }

            if(localCategory.HasSize && Data.FindSize != null)
            {
                Data.Size = Data.FindSize;
                localCategory -= Category.Size;
            }

            if(localCategory.HasExts && Data.FindExts != null)
            {
                Data.Exts = Data.FindExts;
                localCategory -= Category.Exts;
            }

            if(!localCategory.HasAny && !(category & Data.PendingCategory).HasAny)
                return;

            var oldPendingCategory = Data.PendingCategory;
            try
            {
                Data.PendingCategory |= localCategory;
                var result = Provider.Execute(localCategory, oldPendingCategory & category);
                Tracer.Assert(result != null);
                Tracer.Assert(localCategory <= result.CompleteCategory);
                Data.Update(result);
            }
            finally
            {
                Data.PendingCategory = oldPendingCategory - Data.CompleteCategory;
            }
        }

        public static Result operator &(ResultCache resultCache, Category category)
            => resultCache.GetCategories(category);


        internal Result GetCategories(Category category)
        {
            var trace = ObjectId == -11;
            StartMethodDump(trace, category, nameof(Calls), Calls);
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

        //[DebuggerHidden]
        void GuardedUpdate(Category category)
        {
            try
            {
                Current = new CallStack
                {
                    Former = Current,
                    Item = new Call
                    {
                        Category = category,
                        Item = this
                    }
                };

                Update(category);
            }
            finally
            {
                Current = Current.Former;
            }
        }

        [DisableDump]
        internal TypeBase Type => GetCategories(Category.Type).Type;

        [DisableDump]
        internal CodeBase Code => GetCategories(Category.Code).Code;

        [DisableDump]
        internal CodeArgs Exts => GetCategories(Category.Exts).Exts;

        [DisableDump]
        internal Size Size => GetCategories(Category.Size).Size;

        [DisableDump]
        internal bool? Hllw => GetCategories(Category.Hllw).Hllw;

        public override string DumpData()
        {
            var result = FunctionDump;
            if(result != "")
                result += "\n";
            result += Data.DumpData();
            return result;
        }

        IEnumerable<TreeNode> ITreeNodeSupport.CreateNodes() => Data.TreeNodes;
    }
}