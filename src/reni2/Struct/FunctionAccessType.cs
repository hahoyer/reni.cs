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
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionAccessType : TypeBase, ISetterTargetType, IContainerType, IConverter, IReference
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

        [DisableDump]
        RefAlignParam RefAlignParam { get { return _functionalBodyType.RefAlignParam; } }
        public Size RefSize { get { throw new NotImplementedException(); } }
        [DisableDump]
        TypeBase ISetterTargetType.ValueType { get { return ValueType; } }
        [DisableDump]
        IReferenceInCode ISetterTargetType.ObjectReference
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        public TypeBase Type { get { throw new NotImplementedException(); } }

        Result ISetterTargetType.Result(Category category) { return _functionalBodyType.SetterResult(category, _argsType); }
        Result ISetterTargetType.DestinationResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
        [DisableDump]
        IConverter IContainerType.Converter { get { return this; } }
        [DisableDump]
        TypeBase IContainerType.Target { get { return ValueType; } }
        [DisableDump]
        internal override bool IsDataLess { get { return CodeArgs.IsNone && _argsType.IsDataLess; } }
        [DisableDump]
        internal override bool IsLikeReference { get { return true; } }
        [DisableDump]
        CodeArgs CodeArgs
        {
            get
            {
                return _functionalBodyType
                    .GetCodeArgs(_argsType);
            }
        }
        [DisableDump]
        TypeBase ValueType { get { return _functionalBodyType.ValueType(_argsType); } }
        TypeBase IReference.Type { get { return this; } }
        TypeBase IReference.TargetType { get { return ValueType; } }
        RefAlignParam IReference.RefAlignParam { get { return RefAlignParam; } }

        Result IConverter.Result(Category category)
        {
            var trace = ObjectId == -6 && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var result = _functionalBodyType.GetterResult(category, _argsType);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result IReference.DereferenceResult(Category category) { return _functionalBodyType.GetterResult(category, _argsType); }

        protected override Size GetSize() { return CodeArgs.Size + _argsType.Size; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.SearchAtPath(this);
            searchVisitor.SearchWithPath(ValueType, this);
            searchVisitor.SearchAndConvert(ValueType, this);
            base.Search(searchVisitor);
        }

        internal Result AssignmentFeatureResult(Category category)
        {
            var result = new Result
                (category
                 , () => false
                 , () => RefAlignParam.RefSize
                 , () => _setterTypeCache.Find(RefAlignParam)
                 , ArgCode
                 , CodeArgs.Arg
                );
            return result;
        }
        RefAlignParam IReferenceInCode.RefAlignParam
        {
            get { return RefAlignParam; } }
    }
}