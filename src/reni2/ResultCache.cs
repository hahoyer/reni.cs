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
        readonly Result _data = new Result();
        readonly Func<Category, Result> _obtainResult;
        internal string FunctionDump = "";

        public ResultCache(Func<Category, Result> obtainResult) { _obtainResult = obtainResult ?? NotSupported; }

        ResultCache(Result data)
        {
            _data = data;
            _obtainResult = NotSupported;
        }

        public static implicit operator ResultCache(Result x) { return new ResultCache(x); }

        Result NotSupported(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result Data { get { return _data; } }

        //[DebuggerHidden]
        internal void Update(Category category)
        {
            var localCategory = category - _data.CompleteCategory - _data.PendingCategory;

            if(localCategory.HasHllw && _data.FindHllw != null)
            {
                _data.Hllw = _data.FindHllw;
                localCategory -= Category.Hllw;
            }

            if(localCategory.HasSize && _data.FindSize != null)
            {
                _data.Size = _data.FindSize;
                localCategory -= Category.Size;
            }

            if(localCategory.HasExts && _data.FindArgs != null)
            {
                _data.Exts = _data.FindArgs;
                localCategory -= Category.Exts;
            }

            if(localCategory.HasAny)
            {
                var oldPendingCategory = _data.PendingCategory;
                try
                {
                    _data.PendingCategory |= localCategory;
                    var result = _obtainResult(localCategory);
                    Tracer.Assert(localCategory <= result.CompleteCategory);
                    _data.Update(result);
                }
                finally
                {
                    _data.PendingCategory = oldPendingCategory;
                }
            }
        }

        public static Result operator &(ResultCache resultCache, Category category)
        {
            return resultCache.GetCategories(category);
        }

        Result GetCategories(Category category)
        {
            Update(category);
            Tracer.Assert(category <= Data.CompleteCategory);
            return Data & category;
        }

        bool HasHllw
        {
            get { return _data.HasHllw; }
            set
            {
                if(value)
                    Update(Category.Hllw);
                else
                    _data.Hllw = null;
            }
        }

        bool HasSize
        {
            get { return _data.HasSize; }
            set
            {
                if(value)
                    Update(Category.Size);
                else
                    _data.Size = null;
            }
        }

        bool HasType
        {
            get { return _data.HasType; }
            set
            {
                if(value)
                    Update(Category.Type);
                else
                    _data.Type = null;
            }
        }

        internal bool HasCode
        {
            get { return _data.HasCode; }
            set
            {
                if(value)
                    Update(Category.Code);
                else
                    _data.Code = null;
            }
        }

        bool HasCodeArgs
        {
            get { return _data.HasExts; }
            set
            {
                if(value)
                    Update(Category.Exts);
                else
                    _data.Exts = null;
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

        IEnumerable<TreeNode> ITreeNodeSupport.CreateNodes() { return _data.TreeNodes; }
    }
}