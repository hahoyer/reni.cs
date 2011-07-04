using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class StructureType : TypeBase
    {
        private readonly Structure _structure;

        [DisableDump]
        internal readonly ISearchPath<IFeature, AutomaticReferenceType> DumpPrintReferenceFeature;

        internal StructureType(Structure structure)
        {
            _structure = structure;
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Structure.RefAlignParam; } }

        [DisableDump]
        internal ContainerContextObject ContainerContextObject { get { return Structure.ContainerContextObject; } }

        [DisableDump]
        internal Structure Structure { get { return _structure; } }

        protected override Size GetSize() { return Structure.StructSize; }

        internal override string DumpShort() { return "type(" + Structure.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<IFeature>;
            if(searchVisitorChild != null && !searchVisitorChild.IsSuccessFull)
            {
                searchVisitorChild.InternalResult =
                    Structure
                        .SearchFromRefToStruct(searchVisitorChild.Defineable)
                        .CheckedConvert(this);
            }
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected Result VirtualForceConversion(Category category, TypeBase destination)
        {
            Tracer.Assert(destination.IsVoid);
            Tracer.Assert(Size.IsZero);
            return destination.Result
                (category
                , destination.ArgCode
                , () => Structure.ConstructorRefs
                );
        }

        internal override Structure GetStructure() { return Structure; }

        internal bool VirtualIsConvertable(TypeBase destination, ConversionParameter conversionParameter)
        {
            if(destination.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(destination, conversionParameter);
            return false;
        }

        internal CodeBase LocalCode()
        {
            return LocalReferenceCode(RefAlignParam)
                .Dereference(RefAlignParam, Size);
        }

        internal Result DumpPrintResult(Category category) { return Structure.ReplaceContextReferenceByThisReference(category); }
    }
}