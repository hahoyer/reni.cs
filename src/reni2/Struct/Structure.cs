using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class AccessPoint : ReniObject
    {
        private readonly ContainerContextObject _containerContextObject;
        private readonly int _position;

        internal AccessPoint(ContainerContextObject containerContextObject, int position)
        {
            _containerContextObject = containerContextObject;
            _position = position;
        }

        [EnableDump]
        internal int Position { get { return _position; } }

        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }
    }

    internal sealed class Structure : ReniObject
    {
        private readonly ContainerContextObject _containerContextObject;
        private readonly int _endPosition;
        private readonly DictionaryEx<ICompileSyntax, FunctionalFeatureType> _functionalFeatureCache;
        private readonly SimpleCache<StructureType> _typeCache;

        internal Structure(ContainerContextObject containerContextObject, int endPosition)
        {
            _containerContextObject = containerContextObject;
            _endPosition = endPosition;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalFeatureType>(body => new FunctionalFeatureType(this, body));
            _typeCache = new SimpleCache<StructureType>(() => new StructureType(this));
        }

        [EnableDump]
        internal int EndPosition { get { return _endPosition; } }

        [EnableDump]
        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }

        [DisableDump]
        internal ContextBase SpawnContext { get { return ContainerContextObject.SpawnContext(_endPosition); } }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        private Reference ReferenceType { get { return Type.Reference(RefAlignParam); } }

        [DisableDump]
        internal Refs ConstructorRefs
        {
            get
            {
                return ContainerContextObject
                    .Container
                    .InnerResult(Category.Refs, ContainerContextObject.Parent, EndPosition).Refs;
            }
        }

        [DisableDump]
        internal TypeBase InnerType { get { return ContainerContextObject.InnerType(EndPosition); } }

        [DisableDump]
        internal Size InnerSize { get { return ContainerContextObject.InnerSize(EndPosition); } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        [DisableDump]
        internal Size StructSize { get { return ContainerContextObject.StructSize(EndPosition); } }


        internal Result FunctionalResult(Category category, ICompileSyntax body)
        {
            return _functionalFeatureCache
                .Find(body)
                .Result(category);
        }

        internal ISearchPath<IFeature, StructureType> SearchFromRefToStruct(Defineable defineable)
        {
            return ContainerContextObject
                .Container
                .SearchFromRefToStruct(defineable);
        }

        internal Result CreateFunctionCall(Category category, ICompileSyntax body, Result argsResult)
        {
            return ContainerContextObject
                .RootContext
                .CreateFunctionCall(this, category, body, argsResult);
        }

        internal Result Access(Category category, Result thisReferenceResult, Result rightResult) { return AccessFromThisReference(category, thisReferenceResult, rightResult.ConvertTo(IndexType).Evaluate().ToInt32()); }

        internal Result FieldAccess(Category category, int position)
        {
            Tracer.Assert(position < EndPosition);
            return ContainerContextObject.FieldAccessFromContextReference(category, position);
        }

        internal Result FunctionAccess(Category category, int position)
        {
            return ContainerContextObject.FunctionAccessFromContextReference(category, position);
        }

        internal Result DumpPrintResultFromContextReference(Category category)
        {
            var result = Result
                .ConcatPrintResult(category, EndPosition, position => DumpPrintResultFromThisReference(category, position));
            return result;
        }

        private Result DumpPrintResultFromThisReference(Category category, int position)
        {
            return ContainerContextObject
                .InnerType(position)
                .GenericDumpPrint(category,RefAlignParam)
                .ReplaceArg(AccessViaThisReference(category, position));
        }

        private Result AccessFromThisReference(Category category, Result thisReferenceResult, int position)
        {
            return AccessViaThisReference(category, position).ReplaceArg(thisReferenceResult);
        }

        internal Result ReplaceContextReferenceByThisReference(Result result)
        {
            return ContainerContextObject.ReplaceContextReferenceByThisReference(EndPosition, result);
        }

        internal Result AccessViaThisReference(Category category, int position)
        {
            return ReplaceContextReferenceByThisReference(AccessViaContextReference(category, position));
        }

        internal Result AccessViaContextReference(Category category, int position)
        {
            return ContainerContextObject
                .SpawnAccessObject(position)
                .AccessViaContextReference(category, this, position);
        }

        internal Result ThisReferenceFromContextReference(Category category)
        {
            return ContainerContextObject
                .AccessFromContextReference(category, EndPosition, () => Type.Reference(RefAlignParam));
        }

        internal Result ReplaceContextReferenceByThisReference(Category category)
        {
            return ReplaceContextReferenceByThisReference(DumpPrintResultFromContextReference(category));
        }
    }
}