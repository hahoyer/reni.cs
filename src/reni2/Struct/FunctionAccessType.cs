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
    sealed class FunctionAccessType : TypeBase
    {
        [EnableDump]
        readonly FunctionalBodyType _functionalBodyType;
        [EnableDump]
        readonly TypeBase _argsType;

        public FunctionAccessType(FunctionalBodyType functionalBodyType, TypeBase argsType)
        {
            _functionalBodyType = functionalBodyType;
            _argsType = argsType;
        }

        [DisableDump]
        internal override bool IsDataLess { get { return _argsType.IsDataLess; } }
        [DisableDump]
        internal override bool IsLikeReference { get { return true; } }

        protected override Size GetSize() { return _argsType.Size; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result AssignmentFeatureResult(Category category, RefAlignParam refAlignParam)
        {
            var result = new Result
                (category
                 , () => false
                 , () => refAlignParam.RefSize
                 , () => _assignmentFeatureCache.Value.UniqueFunctionalType(refAlignParam)
                 , ArgCode
                 , CodeArgs.Arg
                );
            return result;
        }
    }
}