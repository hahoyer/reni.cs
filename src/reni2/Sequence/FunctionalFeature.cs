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
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class FunctionalFeature : ReniObject, IFeature, IFunctionalFeature
    {
        private readonly SequenceType _parent;

        [EnableDump]
        private readonly FeatureBase _feature;

        internal FunctionalFeature(SequenceType parent, FeatureBase feature)
        {
            _parent = parent;
            _feature = feature;
        }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam)
        {
            return new Result
                (category
                 , () => refAlignParam.RefSize
                 , () => _parent.SpawnReference(refAlignParam).SpawnFunctionalType(this)
                 , () => _parent.SpawnReference(refAlignParam).ArgCode()
                );
        }

        string IDumpShortProvider.DumpShort() { return _feature.Definable.DataFunctionName; }

        private Result Apply(Category category, int objSize, int argsSize, RefAlignParam refAlignParam)
        {
            var type = _feature.ResultType(objSize, argsSize);
            return type.Result(category, () => BitSequenceOperation(type.Size, _feature.Definable, objSize, argsSize, refAlignParam));
        }

        TypeBase IFeature.DefiningType() { return _parent; }

        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            var typeedCategory = category | Category.Type;
            var result = Apply(category, _parent.Count, argsType.SequenceCount(_parent.Element), refAlignParam);
            var objectResult = _parent.ObjectReferenceInCode(typeedCategory, refAlignParam);
            var convertedObjectResult = objectResult.ConvertToBitSequence(typeedCategory);
            var convertedArgsResult = argsType.ConvertToBitSequence(typeedCategory);
            return result.ReplaceArg(convertedObjectResult.Pair(convertedArgsResult));
        }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }

        private static CodeBase BitSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, int objBits, int argsBits, RefAlignParam refAlignParam)
        {
            var objSize = Size.Create(objBits);
            var argsSize = Size.Create(argsBits);
            var operandsSize = objSize.ByteAlignedSize + argsSize.ByteAlignedSize;
            return TypeBase
                .Number(operandsSize.ToInt())
                .SpawnReference(refAlignParam)
                .ArgCode()
                .Dereference(refAlignParam, operandsSize)
                .BitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }
    }
}