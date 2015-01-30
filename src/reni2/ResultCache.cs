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
        Func<Category, Result> ObtainResult { get; }
        internal string FunctionDump = "";

        public ResultCache(Func<Category, Result> obtainResult) { ObtainResult = obtainResult ?? NotSupported; }

        ResultCache(Result data)
        {
            Data = data;
            ObtainResult = NotSupported;
        }

        public static implicit operator ResultCache(Result x) => new ResultCache(x);

        Result NotSupported(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result Data { get; } = new Result();

        //[DebuggerHidden]
        internal void Update(Category category)
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

            if(localCategory.HasAny)
            {
                var oldPendingCategory = Data.PendingCategory;
                try
                {
                    Data.PendingCategory |= localCategory;
                    var result = ObtainResult(localCategory);
                    Tracer.Assert(localCategory <= result.CompleteCategory);
                    Data.Update(result);
                }
                finally
                {
                    Data.PendingCategory = oldPendingCategory;
                }
            }
        }

        public static Result operator &(ResultCache resultCache, Category category) => resultCache.GetCategories(category);

        Result GetCategories(Category category)
        {
            Update(category);
            Tracer.Assert(category <= Data.CompleteCategory);
            return Data & category;
        }

        bool HasHllw
        {
            get { return Data.HasHllw; }
            set
            {
                if(value)
                    Update(Category.Hllw);
                else
                    Data.Hllw = null;
            }
        }

        bool HasSize
        {
            get { return Data.HasSize; }
            set
            {
                if(value)
                    Update(Category.Size);
                else
                    Data.Size = null;
            }
        }

        bool HasType
        {
            get { return Data.HasType; }
            set
            {
                if(value)
                    Update(Category.Type);
                else
                    Data.Type = null;
            }
        }

        internal bool HasCode
        {
            get { return Data.HasCode; }
            set
            {
                if(value)
                    Update(Category.Code);
                else
                    Data.Code = null;
            }
        }

        bool HasCodeArgs
        {
            get { return Data.HasExts; }
            set
            {
                if(value)
                    Update(Category.Exts);
                else
                    Data.Exts = null;
            }
        }

        [DisableDump]
        internal TypeBase Type
        {
            get
            {
                Update(Category.Type);
                return Data.Type;
            }
        }
        [DisableDump]
        internal CodeBase Code
        {
            get
            {
                Update(Category.Code);
                return Data.Code;
            }
        }
        [DisableDump]
        internal CodeArgs CodeArgs
        {
            get
            {
                Update(Category.Exts);
                return Data.Exts;
            }
        }
        [DisableDump]
        internal Size Size
        {
            get
            {
                Update(Category.Size);
                return Data.Size;
            }
        }
        [DisableDump]
        internal bool? Hllw
        {
            get
            {
                Update(Category.Hllw);
                return Data.Hllw;
            }
        }

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