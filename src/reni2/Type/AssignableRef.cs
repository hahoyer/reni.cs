using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    internal sealed class AssignableRef : TypeBase
    {
        private readonly Struct.Context _context;
        private readonly int _position;
        private readonly SimpleCache<TypeBase> _targetCache;
        [DumpData(false)]
        internal readonly AssignmentFeature AssignmentFeature;

        public AssignableRef(Struct.Context context, int position)
        {
            _context = context;
            _position = position;
            _targetCache = new SimpleCache<TypeBase>(GetTargetType);
            AssignmentFeature = new AssignmentFeature(this);
        }

        internal RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        internal Size TargetSize { get { return _targetCache.Value.Size; } }
        protected override Size GetSize() { return _context.RefSize; }
        internal override string DumpShort() { return "type(this at " + _position + ")"; }
        internal override bool IsValidRefTarget() { return false; }

        private TypeBase GetTargetType()
        {
            NotImplementedMethod();
            return null;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _targetCache.Value.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        internal AutomaticRef CreateAutomaticRef()
        {
            return _targetCache
                .Value
                .CreateAutomaticRef(_context.RefAlignParam);
        }
    }

    [Serializable]
    internal class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature
    {
        [DumpData(true)]
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef) { _assignableRef = assignableRef; }

        Result IFeature.Apply(Category category) { return _assignableRef.CreateFunctionalType(this).CreateArgResult(category); }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var result = TypeBase
                .CreateVoid
                .CreateResult
                (
                    category,
                    () =>
                    CodeBase
                        .CreateArg(_assignableRef.Size*2)
                        .CreateAssignment(_assignableRef.RefAlignParam, _assignableRef.TargetSize)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsResult.ConvertToAsRef(category, _assignableRef.CreateAutomaticRef());
            var destinationResult = functionalResult.StripFunctional() & category;
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        string IDumpShortProvider.DumpShort() { return _assignableRef.DumpShort() + " :="; }
    }
}