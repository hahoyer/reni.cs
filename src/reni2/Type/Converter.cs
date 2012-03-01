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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Type
{
    interface IConverter
    {
        Result Result(Category category);
    }

    static class ConverterExtension
    {
        public static IConverter Concat(this IConverter first, Func<Category, Result> second) { return new ConcatConverter(first, new FunctionalConverter(second)); }
        public static IConverter Concat(this IConverter first, IConverter second) { return new ConcatConverter(first, second); }
    }

    sealed class ConcatConverter : ReniObject, IConverter
    {
        readonly IConverter _first;
        readonly IConverter _second;

        public ConcatConverter(IConverter first, IConverter second)
        {
            _first = first;
            _second = second;
        }

        public Result Result(Category category)
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

    sealed class FunctionalConverter : IConverter
    {
        readonly Func<Category, Result> _function;

        public FunctionalConverter(Func<Category, Result> function) { _function = function; }
        public Result Result(Category category) { return _function(category); }
    }
}