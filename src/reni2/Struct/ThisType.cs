using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    internal class Type : TypeBase, IFeatureTarget
    {
        private static int _nextObjectId;
        [DumpData(true)]
        private readonly Context _context;

        [DumpData(false)]
        internal readonly IFeature DumpPrintFeature;
        [DumpData(false)]
        internal readonly ISearchPath<IFeature, Reni.Type.Reference> DumpPrintReferenceFeature;

        internal Type(Context context)
            : base(_nextObjectId++)
        {
            _context = context;
            DumpPrintFeature = new Feature.DumpPrint.Feature<Type>(this);
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        internal RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        internal Context Context { get { return _context; } }

        protected override Size GetSize() { return _context.InternalSize(); }
        internal override string DumpShort() { return "type(" + _context.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<IFeature>;
            if (searchVisitorChild != null)
            {
                searchVisitorChild.InternalResult =
                    _context
                    .Container
                    .SearchFromRefToStruct(searchVisitorChild.Defineable)
                    .CheckedConvert(this);
            }
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal override Context GetStruct() { return _context; }

        Result IFeatureTarget.Apply(Category category)
        {
            var argCodes = CreateArgCodes(category);
            var dumpPrint =
                Context.Types
                    .Select((type, i) => type.GenericDumpPrint(category).UseWithArg(argCodes[i]))
                    .ToArray();
            var thisRef = CreateArgResult(category)
                .CreateAutomaticRefResult(Context.RefAlignParam);
            var result = Result
                .ConcatPrintResult(category, dumpPrint)
                .UseWithArg(thisRef);
            return result;
        }

        private Result[] CreateArgCodes(Category category)
        {
            return Context.Types
                .Select((type, i) => AutomaticDereference(type, Context.Offsets[i], category))
                .ToArray();
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            return type
                .CreateReference(Context.RefAlignParam)
                .CreateResult(category, () => CreateRefArgCode().CreateRefPlus(Context.RefAlignParam, offset))
                .AutomaticDereference();
        }

        private CodeBase CreateRefArgCode() { return CreateReference(Context.RefAlignParam).CreateArgCode(); }
    }
}