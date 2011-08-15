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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Type
{
    internal sealed class ConcatArraysFeature : TypeBase, IFunctionalFeature
    {
        [EnableDump]
        private readonly Array _type;
        private readonly Size _refSize;

        public ConcatArraysFeature(Array type, Size refSize)
        {
            _type = type;
            _refSize = refSize;
        }

        protected override Size GetSize() { return _refSize; }

        Result IFunctionalFeature.ObtainApplyResult(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam)
        {
            var newCount = argsResult.Type.ArrayElementCount;
            var newElementResult = argsResult.Conversion(argsResult.Type.IsArray ? _type.Element.UniqueArray(newCount) : _type.Element);
            return _type
                .Element
                .UniqueArray(_type.Count + newCount)
                .Result
                (category
                 , () => newElementResult.Code.Sequence(operationResult.Code.Dereference(refAlignParam, _type.Size))
                 , () => newElementResult.Refs + operationResult.Refs
                );
        }
    }
}