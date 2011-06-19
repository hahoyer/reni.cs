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
        private readonly DictionaryEx<ICompileSyntax, FunctionalFeatureType> _functionalFeatureCache;
        private readonly SimpleCache<AccessPointType> _typeCache;

        internal AccessPoint(ContainerContextObject containerContextObject, int position)
        {
            _containerContextObject = containerContextObject;
            _position = position;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalFeatureType>(body => new FunctionalFeatureType(this, body));
            _typeCache = new SimpleCache<AccessPointType>(() => new AccessPointType(this));
        }

        [EnableDump]
        internal int Position { get { return _position; } }

        [EnableDump]
        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }

        [DisableDump]
        internal ContextBase SpawnContext { get { return ContainerContextObject.SpawnContext(_position); } }

        [DisableDump]
        internal AccessPointType Type { get { return _typeCache.Value; } }

        [DisableDump]
        private Reference ReferenceType { get { return Type.Reference(RefAlignParam); } }

        [DisableDump]
        internal Refs ConstructorRefs
        {
            get
            {
                return ContainerContextObject
                    .Container
                    .InnerResult(Category.Refs, ContainerContextObject.Parent, Position).Refs;
            }
        }

        [DisableDump]
        internal TypeBase InnerType { get { return ContainerContextObject.InnerType(Position); } }

        [DisableDump]
        internal Size InnerSize { get { return ContainerContextObject.InnerSize(Position); } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        [DisableDump]
        internal Size StructSize { get { return ContainerContextObject.StructSize(Position); } }


        internal Result FunctionalResult(Category category, ICompileSyntax body)
        {
            return _functionalFeatureCache
                .Find(body)
                .Result(category);
        }

        internal ISearchPath<IFeature, AccessPointType> SearchFromRefToStruct(Defineable defineable)
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
            Tracer.Assert(position < Position);
            return ContainerContextObject.FieldAccessFromContextReference(category, position);
        }

        internal Result FunctionAccess(Category category, int position)
        {
            return ContainerContextObject.FunctionAccessFromContextReference(category, position);
        }

        internal Result DumpPrintResult(Category category, Result thisRef)
        {
            var containerContextObject = ContainerContextObject;
            var result = Result
                .ConcatPrintResult(category, Position, position => DumpPrintResultFromThisReference(category, containerContextObject, position))
                .ReplaceArg(thisRef);
            return result;
        }

        private Result DumpPrintResultFromThisReference(Category category, ContainerContextObject containerContextObject, int i)
        {
            return containerContextObject
                .InnerType(i)
                .GenericDumpPrint(category)
                .ReplaceArg(AccessViaThisReference(category, i).AutomaticDereference());
        }

        private Result AccessFromThisReference(Category category, Result thisReferenceResult, int position)
        {
            return AccessViaThisReference(category, position).ReplaceArg(thisReferenceResult);
        }

        internal Result ReplaceContextReferenceByThisReference(Result result)
        {
            return ContainerContextObject.ReplaceContextReferenceByThisReference(Position, result);
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
                .AccessFromContextReference(category, Position, () => Type.Reference(RefAlignParam));
        }
    }
}