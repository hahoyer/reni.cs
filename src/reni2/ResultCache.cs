using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni
{
    sealed class ResultCache : DumpableObject, ITreeNodeSupport
    {
        internal interface IResultProvider
        {
            Result Execute(Category category);
            object Target { get; }
        }

        sealed class ResultNotSupported : DumpableObject, IResultProvider
        {
            Result IResultProvider.Execute(Category category)
            {
                NotImplementedMethod(category);
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

            Result IResultProvider.Execute(Category category) => ObtainResult(category);
            object IResultProvider.Target => null;
        }

        static readonly IResultProvider NotSupported = new ResultNotSupported();
        IResultProvider Provider { get; }
        internal string FunctionDump = "";

        internal ResultCache(IResultProvider obtainResult)
        {
            Provider = obtainResult ?? NotSupported;
        }

        internal ResultCache(Func<Category, Result> obtainResult)
        {
            Provider = new SimpleProvider(obtainResult);
        }

        ResultCache(Result data)
        {
            Data = data;
            Provider = NotSupported;
        }

        public static implicit operator ResultCache(Result x) => new ResultCache(x);

        internal Result Data { get; } = new Result();

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

            if(localCategory.HasExts && Data.FindArgs != null)
            {
                Data.Exts = Data.FindArgs;
                localCategory -= Category.Exts;
            }

            if(!localCategory.HasAny)
                return;

            var oldPendingCategory = Data.PendingCategory;
            try
            {
                Data.PendingCategory |= localCategory;
                var result = Provider.Execute(localCategory);
                Tracer.Assert(localCategory <= result.CompleteCategory);
                Data.Update(result);
            }
            finally
            {
                Data.PendingCategory = oldPendingCategory;
            }
        }

        public static Result operator &(ResultCache resultCache, Category category)
            => resultCache.GetCategories(category);

        internal Result GetCategories(Category category)
        {
            var trace = true;
            StartMethodDump(trace, category, nameof(Provider), Provider);
            try
            {
                Update(category);
                return ReturnMethodDump(Data & category);
            }
            finally
            {
                EndMethodDump();
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