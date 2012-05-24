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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Sequence;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, IReference objectReference)
        {
            return TypeBase.Void
                .Result(category, () => BitSequenceDumpPrint(objectReference), CodeArgs.Arg);
        }

        static CodeBase BitSequenceDumpPrint(IReference objectReference)
        {
            var alignedSize = objectReference.TargetType.Size.Align(objectReference.RefAlignParam.AlignBits);
            return objectReference
                .Type
                .ArgCode()
                .Dereference(alignedSize)
                .DumpPrintNumber(alignedSize);
        }
    }

    sealed class BitSequenceFeature :
        ReniObject,
        ISearchPath<ISuffixFeature, SequenceType>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type) { return type.BitDumpPrintFeature; }
    }

    sealed class BitSequenceFeatureClass : BitFeatureBase, ISuffixFeature
    {
        readonly SequenceType _parent;

        internal BitSequenceFeatureClass(SequenceType parent) { _parent = parent; }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return Apply(category, _parent.UniqueReference(refAlignParam)); }
        [EnableDump]
        internal TypeBase ObjectType { get { return _parent; } }
    }

    sealed class BitFeature : BitFeatureBase, ISuffixFeature
    {
        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return Apply(category, TypeBase.Bit.UniqueReference(refAlignParam)); }
    }

    sealed class StructReferenceFeature : ReniObject, ISearchPath<ISuffixFeature, AutomaticReferenceType>,
                                          ISuffixFeature
    {
        [EnableDump]
        readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        ISuffixFeature ISearchPath<ISuffixFeature, AutomaticReferenceType>.Convert(AutomaticReferenceType type)
        {
            Tracer.Assert(type.RefAlignParam == _structureType.RefAlignParam);
            return this;
        }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam)
        {
            return _structureType
                .Structure
                .DumpPrintResultViaContextReference(category);
        }
    }
}