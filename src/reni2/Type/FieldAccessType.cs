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
using Reni.Struct;

namespace Reni.Type
{
    sealed class FieldAccessType : Child<TypeBase>, ISetterTargetType
    {
        readonly RefAlignParam _refAlignParam;
        readonly Size _offset;
        readonly DictionaryEx<RefAlignParam, TypeBase> _setterTypeCache;

        internal FieldAccessType(TypeBase target, RefAlignParam refAlignParam, Size offset)
            : base(target)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
            _setterTypeCache = new DictionaryEx<RefAlignParam, TypeBase>(rap => new SetterType(this, rap));
        }
        protected override Size GetSize() { return _refAlignParam.RefSize; }
        internal override bool IsDataLess { get { return false; } }
        internal override TypeBase SmartReference(RefAlignParam refAlignParam) { return this; }

        internal override Result SmartLocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            return UniqueAlign(refAlignParam.AlignBits)
                .Result
                (category
                 , () => LocalReferenceCode(refAlignParam).Dereference(refAlignParam, refAlignParam.RefSize)
                 , () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        protected override Result ParentConversionResult(Category category)
        {
            return Parent.SmartReference(_refAlignParam)
                .Result(category, () => ArgCode().AddToReference(_refAlignParam, _offset), CodeArgs.Arg);
        }

        public Result AssignmentFeatureResult(Category category, RefAlignParam refAlignParam)
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
        
        TypeBase ISetterTargetType.ValueType { get { return Parent; } }
        
        Result ISetterTargetType.ApplySetterResult(Category category, TypeBase valueType)
        {
            NotImplementedMethod(category, valueType);
            return null;
        }
    }
}