﻿#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    sealed class FunctionFeature : FunctionalFeature, ISuffixFeature, IFunctionFeature
    {
        readonly SequenceType _objectType;

        [EnableDump]
        readonly FeatureBase _feature;

        internal FunctionFeature(SequenceType objectType, FeatureBase feature)
        {
            _objectType = objectType;
            _feature = feature;
        }

        [DisableDump]
        internal override IContextReference ObjectReference { get { return GetObjectReference(); } }
        [DisableDump]
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }
        [DisableDump]
        bool IFunctionFeature.IsImplicit { get { return false; } }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return this; } }
        ISimpleFeature IFeature.Simple { get { return null; } }

        internal override string DumpShort() { return base.DumpShort() + " " + _feature.Definable.DataFunctionName; }

        Result Result(Category category)
        {
            return UniqueFunctionalType()
                .Result(category, _objectType.ReferenceArgResult(category.Typed));
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return ApplyResult(category, argsType); }

        internal override Result ApplyResult(Category category, TypeBase argsType)
        {
            Tracer.Assert(_objectType.Element == TypeBase.Bit);
            var typedCategory = category.Typed;
            var result = Apply(category, _objectType.Count, argsType.SequenceCount(_objectType.Element));
            var objectResult = GetObjectReference().Result(typedCategory);
            var convertedObjectResult = objectResult.ConvertToBitSequence(typedCategory);
            var convertedArgsResult = argsType.ConvertToBitSequence(typedCategory);
            return result.ReplaceArg(convertedObjectResult + convertedArgsResult);
        }

        ObjectReference GetObjectReference() { return _objectType.UniqueObjectReference(Root.DefaultRefAlignParam); }

        Result Apply(Category category, int objSize, int argsSize)
        {
            var type = _feature.ResultType(objSize, argsSize);
            return type.Result(category, () => Bit.BitSequenceOperation(type.Size, _feature.Definable, objSize, argsSize), CodeArgs.Arg);
        }
    }
}