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
        private static int _nextObjectId;
        private readonly Structure _structure;

        [DisableDump]
        internal readonly ISearchPath<IFeature, AutomaticReferenceType> DumpPrintReferenceFeature;

        internal StructureType(Structure structure)
            : base(_nextObjectId++)
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

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.Result
                (
                    category,
                    () => CodeBase.Arg(Size.Zero),
                    () => Structure.ConstructorRefs
                );
        }

        internal override Structure GetStructure() { return Structure; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionParameter);
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