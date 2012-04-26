// 
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Struct
{
    sealed class SetterType : TypeBase
    {
        [EnableDump]
        readonly ISetterTargetType _target;
        [EnableDump]
        readonly RefAlignParam _refAlignParam;

        public SetterType(ISetterTargetType target, RefAlignParam refAlignParam)
        {
            _target = target;
            _refAlignParam = refAlignParam;
        }
        internal override bool IsDataLess { get { return false; } }
        protected override Size GetSize() { return _refAlignParam.RefSize; }

        Result ApplyResult(Category category, Result argsResult, RefAlignParam refAlignParam)
        {
            var valueType = _target.ValueType ?? argsResult.Type;
            var result = _target
                .Result(category, valueType)
                .ReplaceArg(argsResult.Conversion(valueType.UniqueAutomaticReference(refAlignParam)));
            return result;
        }
    }

    interface ISetterTargetType
    {
        TypeBase ValueType { get; }
        Result Result(Category category, TypeBase valueType);
    }
}