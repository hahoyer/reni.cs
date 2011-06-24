using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Structure : ReniObject
    {
        private readonly ContainerContextObject _containerContextObject;
        private readonly int _endPosition;
        private readonly DictionaryEx<ICompileSyntax, FunctionalBody> _functionalFeatureCache;
        private readonly SimpleCache<StructureType> _typeCache;

        internal Structure(ContainerContextObject containerContextObject, int endPosition)
        {
            _containerContextObject = containerContextObject;
            _endPosition = endPosition;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalBody>(body => new FunctionalBody(this, body));
            _typeCache = new SimpleCache<StructureType>(() => new StructureType(this));
        }

        [EnableDump]
        internal int EndPosition { get { return _endPosition; } }

        [EnableDump]
        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }

        [DisableDump]
        internal ContextBase SpawnContext
        {
            get
            {
                return ContainerContextObject
                    .Parent
                    .SpawnChildContext(ContainerContextObject.Container, EndPosition);
            }
        }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        internal Reference ReferenceType { get { return Type.Reference(RefAlignParam); } }

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
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        [DisableDump]
        internal Size StructSize { get { return ContainerContextObject.StructSize(EndPosition); } }

        internal Result Access(Category category, Result thisReferenceResult, Result rightResult) { return AccessFromThisReference(category, thisReferenceResult, rightResult.ConvertTo(IndexType).Evaluate().ToInt32()); }

        internal Result FunctionAccess(Category category, int position)
        {
            Func<TypeBase> getFunctionalType = () => ReferenceType.FunctionalType(ContainerContextObject.InnerType(position).FunctionalFeature());
            return ContainerContextObject.FunctionAccessFromContextReference(category, getFunctionalType);
        }

        private ICompileSyntax[] Statements { get { return ContainerContextObject.Statements; } }

        internal Result AccessViaThisReference(Category category, int position) { return ReplaceContextReferenceByThisReference(AccessViaContextReference(category, position)); }
        internal Result ReplaceContextReferenceByThisReference(Category category) { return ReplaceContextReferenceByThisReference(DumpPrintResultFromContextReference(category)); }

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

        internal Result FieldAccess(Category category, int position)
        {
            Tracer.Assert(position < EndPosition);
            return ContainerContextObject.FieldAccessFromContextReference(category, position);
        }

        internal Result DumpPrintResultFromContextReference(Category category)
        {
            var result = Result.ConcatPrintResult(category, EndPosition, position => DumpPrintResultFromThisReference(category, position));
            return result;
        }

        internal Result AccessViaContextReference(Category category, int position)
        {
            return ContainerContextObject
                .SpawnAccessObject(position)
                .AccessViaContextReference(category, this, position);
        }

        internal Result ThisReferenceViaContextReference(Category category)
        {
            var result = Type.Reference(RefAlignParam).Result
                (category
                 , ThisReferenceViaContextReferenceCode
                 , () => Refs.Create(ContainerContextObject)
                );
            return result;
        }

        private Result DumpPrintResultFromThisReference(Category category, int position)
        {
            return ContainerContextObject
                .InnerType(position)
                .GenericDumpPrint(category, RefAlignParam)
                .ReplaceArg(AccessViaThisReference(category, position));
        }

        private Result AccessFromThisReference(Category category, Result thisReferenceResult, int position) { return AccessViaThisReference(category, position).ReplaceArg(thisReferenceResult); }
        private Result ReplaceContextReferenceByThisReference(Result result) { return ContainerContextObject.ReplaceContextReferenceByThisReference(EndPosition, result); }
        private CodeBase ThisReferenceViaContextReferenceCode() { return CodeBase.ReferenceCode(ContainerContextObject).AddToReference(RefAlignParam, StructSize*-1); }
    }
}