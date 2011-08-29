//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System.Diagnostics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Type
{
    abstract class Converter : ReniObject
    {
        static int _nextObjectId;
        [EnableDump]
        Result _testResult;

        protected Converter()
            : base(_nextObjectId++) { }

        protected void AssertValid()
        {
            if(!Debugger.IsAttached)
                return;

            _testResult = Result(Category.Type | Category.Code);
        }

        internal abstract Result Result(Category category);
        internal bool IsValid { get { return true; } }

        public static Converter operator *(Converter first, Func<Category, Result> second) { return new ConcatConverter(first, new FunctionalConverter(second)); }
        public static Converter operator *(Func<Category, Result> first, Converter second) { return new ConcatConverter(new FunctionalConverter(first), second); }
        public static Converter operator *(Converter first, Converter second) { return new ConcatConverter(first, second); }
    }

    sealed class ConcatConverter : Converter
    {
        readonly Converter _first;
        readonly Converter _second;
        public ConcatConverter(Converter first, Converter second)
        {
            _first = first;
            _second = second;

            AssertValid();
            StopByObjectId(1);
        }

        internal override Result Result(Category category)
        {
            StartMethodDump(false, category);
            try
            {
                var first = _first.Result(category.Typed);
                var second = _second.Result(category);
                Dump("first", first);
                Dump("second", second);
                BreakExecution();
                return ReturnMethodDump(second.ReplaceArg(first));
            }
            finally
            {
                EndMethodDump();
            }
        }
    }

    sealed class FunctionalConverter : Converter
    {
        readonly Func<Category, Result> _function;

        public FunctionalConverter(Func<Category, Result> function)
        {
            _function = function;
            AssertValid();
        }

        internal override Result Result(Category category) { return _function(category); }
    }
}