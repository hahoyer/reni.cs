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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class FunctionalFeature : Type.FunctionalFeature, ISuffixFeature
    {
        private readonly SequenceType _objectType;

        [EnableDump]
        private readonly FeatureBase _feature;

        internal FunctionalFeature(SequenceType objectType, FeatureBase feature)
        {
            _objectType = objectType;
            _feature = feature;
        }

        protected override TypeBase ObjectType { get { return _objectType; } }

        protected override Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            return result
                .ReplaceAbsolute(_objectType.UniqueObjectReference(refAlignParam), category=>_objectType.ReferenceArgResult(category,refAlignParam));
        }

        internal override string DumpShort() { return base.DumpShort() + " " + _feature.Definable.DataFunctionName; }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam)
        {
            return UniqueFunctionalType(refAlignParam)
                .Result(category, _objectType.ReferenceArgResult(category.Typed, refAlignParam));
        }

        protected override Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            Tracer.Assert(_objectType.Element == TypeBase.Bit);
            var typedCategory = category.Typed;
            var result = Apply(category, _objectType.Count, argsType.SequenceCount(_objectType.Element));
            var objectResult = _objectType.UniqueObjectReference(refAlignParam).Result(typedCategory);
            var convertedObjectResult = objectResult.ConvertToBitSequence(typedCategory);
            var convertedArgsResult = argsType.ConvertToBitSequence(typedCategory);
            return result.ReplaceArg(convertedObjectResult.Sequence(convertedArgsResult));
        }

        private Result Apply(Category category, int objSize, int argsSize)
        {
            var type = _feature.ResultType(objSize, argsSize);
            return type.Result(category, () => Bit.BitSequenceOperation(type.Size, _feature.Definable, objSize, argsSize), CodeArgs.Arg);
        }
    }
}