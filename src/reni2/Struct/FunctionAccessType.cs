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
using HWClassLibrary.Helper;
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
        readonly DictionaryEx<RefAlignParam, TypeBase> _setterTypeCache;

        public FunctionAccessType(FunctionalBodyType functionalBodyType, TypeBase argsType)
        {
            _functionalBodyType = functionalBodyType;
            _argsType = argsType;
            _setterTypeCache = new DictionaryEx<RefAlignParam, TypeBase>(refAlignParam => new SetterType(this, refAlignParam));
        }

        sealed class SetterType : TypeBase, IFunctionalFeature
        {
            [EnableDump]
            readonly FunctionAccessType _functionAccessType;
            [EnableDump]
            readonly RefAlignParam _refAlignParam;

            public SetterType(FunctionAccessType functionAccessType, RefAlignParam refAlignParam)
            {
                _functionAccessType = functionAccessType;
                _refAlignParam = refAlignParam;
            }
            internal override bool IsDataLess { get { return false; } }
            protected override Size GetSize() { return _refAlignParam.RefSize; }

            Result IFunctionalFeature.ApplyResult(Category category, Result argsResult, RefAlignParam refAlignParam)
            {
                var valueType = _functionAccessType.ValueType ?? argsResult.Type;
                var result = _functionAccessType
                    .ApplySetterResult(category, valueType)
                    .ReplaceArg(argsResult.Conversion(valueType));
                return result;
            }
        }

        Result ApplySetterResult(Category category, TypeBase valueType)
        {
            NotImplementedMethod(category, valueType);
            return null;
        }

        TypeBase ValueType { get { return _functionalBodyType.ValueType(_argsType); } }

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
                 , () => _setterTypeCache.Find(refAlignParam)
                 , ArgCode
                 , CodeArgs.Arg
                );
            return result;
        }
    }
}