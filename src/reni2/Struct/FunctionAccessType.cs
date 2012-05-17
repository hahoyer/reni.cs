#region Copyright (C) 2012

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
    sealed class FunctionAccessType : TypeBase, ISetterTargetType, IContainerType, IConverter
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
        IReferenceInCode ISetterTargetType.ObjectReference
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        Result ISetterTargetType.Result(Category category, TypeBase valueType) { return _functionalBodyType.SetterResult(category, _argsType, valueType); }
        TypeBase ISetterTargetType.ValueType { get { return ValueType; } }
        IConverter IContainerType.Converter() { return this; }
        TypeBase IContainerType.Target { get { return ValueType; } }
        Result IConverter.Result(Category category)
        {
            var trace = category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var rawResult = _functionalBodyType.GetterResult(category, _argsType);

                if(CodeArgs.Count > 0)
                {
                    Dump("CodeArgs", CodeArgs);
                    BreakExecution();
                }

                var result = rawResult.ReplaceArg(_argsType.Result(category, ArgResult(category)));
                Dump("result", result);
                BreakExecution();
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

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
        protected override Size GetSize() { return CodeArgs.Size + _argsType.Size; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            ValueType.Search(searchVisitor.Child(this));
            searchVisitor.SearchAndConvert(ValueType, this);
            base.Search(searchVisitor);
        }

        TypeBase ValueType { get { return _functionalBodyType.ValueType(_argsType); } }

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

        internal Result ApplyResult(Category category)
        {
            return Result
                (category
                 , () => CodeArgs.ToCode().Sequence(_argsType.ArgCode())
                 , () => CodeArgs + CodeArgs.Arg()
                );
        }
    }
}