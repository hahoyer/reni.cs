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
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Sequence
{
    [Serializable]
    internal sealed class SequenceType : TypeBase
    {
        private readonly Type.Array _inheritedType;

        [DisableDump]
        internal readonly IFeature BitDumpPrintFeature;
        private readonly DictionaryEx<RefAlignParam, ObjectReference> _objectReferencesCache;

        internal Result EnableCutFeature(Category category, RefAlignParam refAlignParam)
        {
            return new Result
                (category
                 , () => Size
                 , () => new EnableCut(this)
                 , () => ForceReference(refAlignParam).ArgCode().Dereference(refAlignParam, Size)
                 , Refs.Arg
                );
        }

        internal IFeature Feature(FeatureBase featureBase) { return new FunctionalFeature(this, featureBase); }

        internal IPrefixFeature PrefixFeature(ISequenceOfBitPrefixOperation definable) { return new PrefixFeature(this, definable); }

        public SequenceType(TypeBase elementType, int count)
        {
            Tracer.Assert(count > 0, () => "count=" + count);
            _inheritedType = elementType.UniqueArray(count);
            BitDumpPrintFeature = new BitSequenceFeatureClass(this);
            _objectReferencesCache = new DictionaryEx<RefAlignParam, ObjectReference>(refAlignParam => new ObjectReference(this, refAlignParam));
            StopByObjectId(-172);
        }

        [DisableDump]
        [UsedImplicitly]
        internal Type.Array InheritedType { get { return _inheritedType; } }

        protected override Size GetSize() { return _inheritedType.Size; }

        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }

        internal override int SequenceCount(TypeBase elementType) { return elementType == Element ? Count : 1; }

        [DisableDump]
        internal int Count { get { return _inheritedType.Count; } }

        [Node]
        [DisableDump]
        public TypeBase Element { get { return _inheritedType.Element; } }

        internal override string DumpShort() { return base.DumpShort() + "(" + Element.DumpShort() + "*" + Count + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Element.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        private Result ExtendFrom(Category category, int oldCount)
        {
            var result = Result
                (
                    category,
                    () => Element.UniqueSequence(oldCount).ArgCode().BitCast(Size)
                    , Refs.Arg
                );
            return result;
        }

        private Result RemoveElementsAtEnd(Category category, int newCount)
        {
            var destructor = Element.Destructor(category);
            if(!destructor.IsEmpty)
            {
                NotImplementedMethod(category, newCount, "destructor", destructor);
                return null;
            }
            var tempNewCount = Math.Min(Count, newCount);
            var newType = Element.UniqueSequence(tempNewCount);
            var result = newType
                .Result
                (
                    category,
                    () => ArgCode().BitCast(newType.Size)
                    , Refs.Arg
                );
            return result;
        }
        internal override Result Destructor(Category category) { return _inheritedType.Destructor(category); }

        internal override Result Copier(Category category) { return _inheritedType.Copier(category); }

        internal ObjectReference UniqueObjectReference(RefAlignParam refAlignParam) { return _objectReferencesCache.Find(refAlignParam); }

        private static Result Conversion(Category category, SequenceType source, SequenceType destination)
        {
            var result = source.ArgResult(category | Category.Type);
            if(source.Count > destination.Count)
                result = source.RemoveElementsAtEnd(category, destination.Count);

            if(source.Element != destination.Element)
            {
                DumpDataWithBreak("Element type dismatch", "category", category, "source", source, "destination", destination, "result", result);
                return null;
            }

            if(source.Count < destination.Count)
                result = destination.ExtendFrom(category, source.Count).ReplaceArg(result);
            return result;
        }

        internal static Converter Converter(SequenceType source, ConversionParameter conversionParameter, SequenceType destination)
        {
            if(source.Count > destination.Count && conversionParameter.IsDisableCut)
                destination.NotImplementedMethod(source, conversionParameter);
            return new FunctionalConverter(category => Conversion(category, source, destination));
        }
        
        internal Result DumpPrintTextResult(Category category, RefAlignParam refAlignParam)
        {
            return Void.Result
                (category
                 , () => DumpPrintCode(refAlignParam)
                 , Refs.Arg
                );
        }

        private CodeBase DumpPrintCode(RefAlignParam refAlignParam)
        {
            return
                UniqueAutomaticReference(refAlignParam)
                .ArgCode()
                .Dereference(refAlignParam, Size)
                .DumpPrintText(Element.Size);
        }
    }
}