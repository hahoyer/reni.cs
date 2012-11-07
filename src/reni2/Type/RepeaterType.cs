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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    abstract class RepeaterType
        : TypeBase
          , IFeature
          , IFunctionFeature
    {
        [Node]
        readonly TypeBase _elementType;
        [Node]
        readonly int? _count;
        [Node]
        readonly SimpleCache<RepeaterAccessType> _arrayAccessTypeCache;

        protected RepeaterType(TypeBase elementType, int? count)
        {
            _elementType = elementType;
            _count = count;
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.ReferenceType == null);
            Tracer.Assert(!elementType.IsDataLess);
            _arrayAccessTypeCache = new SimpleCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
        }

        [Node]
        internal int Count
        {
            get
            {
                Tracer.Assert(_count != null,_elementType.Dump);
                return _count.Value;
            }
        }
        [Node]
        internal TypeBase ElementType { get { return _elementType; } }
        [DisableDump]
        internal override sealed Root RootContext { get { return _elementType.RootContext; } }
        [DisableDump]
        IContextReference ObjectReference { get { return UniquePointer; } }
        [DisableDump]
        internal TypeBase IndexType { get { return RootContext.BitType.UniqueNumber(IndexSize.ToInt()); } }
        [DisableDump]
        internal Size IndexSize { get { return Size.AutoSize(Count).Align(Root.DefaultRefAlignParam.AlignBits); } }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return this; } }
        ISimpleFeature IFeature.Simple { get { return null; } }
        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return ApplyResult(category, argsType); }
        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }

        Result ApplyResult(Category category, TypeBase argsType)
        {
            var objectResult = UniquePointer
                .Result(category, ObjectReference);

            var argsResult = argsType
                .Conversion(category.Typed, IndexType)
                .DereferencedAlignedResult();

            var result = _arrayAccessTypeCache
                .Value
                .Result(category, objectResult + argsResult);

            return result;
        }

        internal Func<Category, Result> ConvertToReference(int count) { return category => ConvertToReference(category, count); }

        Result ConvertToReference(Category category, int count)
        {
            return ElementType
                .UniqueReference(count)
                .Result(category, UniquePointer.ArgResult);
        }
    }
}