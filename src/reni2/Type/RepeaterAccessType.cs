#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

#endregion

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    sealed class RepeaterAccessType
        : SetterTargetType
    {
        [DisableDump]
        internal readonly RepeaterType RepeaterType;

        internal RepeaterAccessType(RepeaterType repeaterType) { RepeaterType = repeaterType; }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        internal override TypeBase ValueType { get { return RepeaterType.ElementType; } }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize + RepeaterType.IndexSize; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal override Result DestinationResult(Category category) { return Result(category, this); }

        internal override Result SetterResult(Category category)
        {
            return new Result
                (category
                 , getCode: SetterCode
                 , getArgs: CodeArgs.Arg
                );
        }
        internal override Result GetterResult(Category category)
        {
            return ValueType
                .UniquePointer
                .Result(category, () => GetterCode);
        }

        CodeBase SetterCode()
        {
            return Pair(ValueType.SmartPointer)
                .ArgCode
                .ArrayAssignment(ValueType.Size, RepeaterType.IndexSize);
        }

        CodeBase GetterCode
        {
            get
            {
                return ArgCode
                    .ArrayAccess(ValueType.Size, RepeaterType.IndexSize);
            }
        }

        internal Func<Category, Result> ConvertToReference(int count) { return category => ConvertToReference(category, count); }

        Result ConvertToReference(Category category, int count)
        {
            var result = RepeaterType
                .ElementType
                .UniqueReference(count)
                .Result(category, GetterResult);
            var xx = result.Dump();
            return result;
        }
    }
}