#region Copyright (C) 2012

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
using Reni.Sequence;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, IReferenceType objectReferenceType)
        {
            return TypeBase.Void
                .Result(category, () => BitSequenceDumpPrint(objectReferenceType), CodeArgs.Arg);
        }

        static CodeBase BitSequenceDumpPrint(IReferenceType objectReferenceType)
        {
            var alignedSize = objectReferenceType.Converter.TargetType.Size.Align(Root.DefaultRefAlignParam.AlignBits);
            return objectReferenceType.Type().ArgCode
                .Dereference(alignedSize)
                .DumpPrintNumber(alignedSize);
        }
    }

    sealed class BitSequenceFeature
        : ReniObject
          , ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Type.Array>
          , ISearchPath<ISuffixFeature, SequenceType>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type) { return type.BitDumpPrintFeature; }
        ISearchPath<ISuffixFeature, SequenceType> ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Type.Array>.Convert(Type.Array type) { return this; }
    }

    sealed class BitSequenceFeatureClass : BitFeatureBase, ISuffixFeature, ISimpleFeature
    {
        readonly SequenceType _parent;

        internal BitSequenceFeatureClass(SequenceType parent) { _parent = parent; }

        [EnableDump]
        internal TypeBase ObjectType { get { return _parent; } }
        Result ISimpleFeature.Result(Category category) { return Apply(category, _parent.UniqueReferenceType); }
        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
    }

    sealed class BitFeature : BitFeatureBase, ISuffixFeature, ISimpleFeature
    {
        Result ISimpleFeature.Result(Category category) { return Apply(category, TypeBase.Bit.UniqueReferenceType); }
        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
    }

    sealed class StructReferenceFeature
        : ReniObject
          , ISearchPath<ISuffixFeature, PointerType>
          , ISuffixFeature
          , ISimpleFeature
    {
        [EnableDump]
        readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        ISuffixFeature ISearchPath<ISuffixFeature, PointerType>.Convert(PointerType type)
        {
            Tracer.Assert(Root.DefaultRefAlignParam == _structureType.RefAlignParam);
            return this;
        }

        Result ISimpleFeature.Result(Category category)
        {
            return _structureType
                .Structure
                .DumpPrintResultViaContextReference(category);
        }
        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
    }
}