using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class Type : TypeBase
    {
        [DumpData(true)]
        private readonly Context _context;

        internal readonly ISearchPath<IFeature, Reni.Type.Reference> DumpPrintFeature;
        [DumpData(false)]
        internal readonly AssignmentFeature AssignmentFeature;


        protected Type(Context context)
        {
            _context = context;
            DumpPrintFeature = new StructReferenceFeature();
            AssignmentFeature = new AssignmentFeature(this);
        }

        internal RefAlignParam RefAlignParam
        {
            get { return _context.RefAlignParam; } }

        protected override Size GetSize() { return _context.InternalSize(); }
        internal override string DumpShort() { return "type(" + _context.DumpShort() + ")"; }

        internal Result ApplyAssignment(Category category, Result functionalResult, Result argsResult)
        {
            var result = CreateVoid
                .CreateResult
                (
                    category,
                    () => CodeBase.CreateArg(RefAlignParam.RefSize*2).CreateAssignment(RefAlignParam, Size)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsResult.ConvertToAsRef(category, CreateReference(RefAlignParam));
            var destinationResult = functionalResult.StripFunctional() & category;
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }
    }

}