// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni
{
    sealed class ResultCache : ReniObject
    {
        readonly Result _data = new Result();
        readonly Func<Category, bool, Result> _obtainResult;
        readonly Func<Category, Result> _obtainFlatResult;
        internal string FunctionDump = "";

        public ResultCache(Func<Category, bool, Result> obtainResult) { _obtainResult = obtainResult; }
        public ResultCache(Func<Category, Result> obtainResult)
        {
            _obtainResult = ObtainFlatResult;
            _obtainFlatResult = obtainResult;
        }

        Result ObtainFlatResult(Category category, bool isPending)
        {
            if(!isPending)
                return _obtainFlatResult(category);
            NotImplementedMethod(category, isPending);
            return null;
        }

        ResultCache(Result data)
        {
            _data = data;
            _obtainResult = NotSupported;
        }

        public static implicit operator ResultCache(Result x) { return new ResultCache(x); }

        Result NotSupported(Category category, bool isPending)
        {
            NotImplementedMethod(category, isPending);
            return null;
        }

        internal Result Data { get { return _data; } }

        //[DebuggerHidden]
        internal void Update(Category category)
        {
            var localCategory = category - _data.CompleteCategory - _data.PendingCategory;

            if(localCategory.HasIsDataLess && _data.FindIsDataLess != null)
            {
                _data.IsDataLess = _data.FindIsDataLess;
                localCategory -= Category.IsDataLess;
            }

            if(localCategory.HasSize && _data.FindSize != null)
            {
                _data.Size = _data.FindSize;
                localCategory -= Category.Size;
            }

            if(localCategory.HasArgs && _data.FindArgs != null)
            {
                _data.CodeArgs = _data.FindArgs;
                localCategory -= Category.CodeArgs;
            }

            if(localCategory.HasAny)
            {
                var oldPendingCategory = _data.PendingCategory;
                try
                {
                    _data.PendingCategory |= localCategory;
                    var result = _obtainResult(localCategory, false);
                    Tracer.Assert(localCategory <= result.CompleteCategory);
                    _data.Update(result);
                }
                finally
                {
                    _data.PendingCategory = oldPendingCategory;
                }
            }

            var pendingCategory = category - _data.CompleteCategory;
            if(pendingCategory.HasAny)
            {
                var pendingResult = _obtainResult(pendingCategory, true);
                Tracer.Assert(pendingResult.CompleteCategory == pendingCategory);
                _data.Update(pendingResult);
            }
        }

        bool HasIsDataLess
        {
            get { return _data.HasIsDataLess; }
            set
            {
                if(value)
                    Update(Category.IsDataLess);
                else
                    _data.IsDataLess = null;
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
            get { return _data.HasArgs; }
            set
            {
                if(value)
                    Update(Category.CodeArgs);
                else
                    _data.CodeArgs = null;
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
                Update(Category.CodeArgs);
                return Data.CodeArgs;
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
        internal bool? IsDataLess
        {
            get
            {
                Update(Category.IsDataLess);
                return Data.IsDataLess;
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
    }
}