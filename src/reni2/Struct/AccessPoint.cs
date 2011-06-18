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
        internal Size StructSize { get { return ContainerContextObject.InnerOffset(Position); } }

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
        internal TypeBase InnerType { get { return ContainerContextObject.InnerType(_position); } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

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

        internal Result[] CreateArgCodes(Category category)
        {
            var result = new Result[Position];
            var offset = StructSize;
            for(var i = 0; i < Position; i++)
            {
                offset -= ContainerContextObject.InnerSize(i).Align(RefAlignParam.AlignBits);
                result[i] = AutomaticDereference(ContainerContextObject.InnerType(i), offset, category);
            }
            return result;
        }

        internal Result Access(Category category, Result thisReferenceResult, Result rightResult) { return AccessFromThisReference(category, thisReferenceResult, rightResult.ConvertTo(IndexType).Evaluate().ToInt32()); }

        internal Result FieldAccess(Category category, int position, bool isContextFeature)
        {
            var result = ContainerContextObject.FieldAccessFromThisReference(category, Position, position);
            if(isContextFeature)
                return result.ReplaceArg(ThisReferenceFromContextReferenceResult(category - Category.Type));
            return result;
        }

        internal Result ThisReferenceFromContextReferenceResult(Category category)
        {
            return ReferenceType
                .Result(category, ThisReferenceFromContextReferenceCode, () => Refs.Create(ContainerContextObject));
        }

        private Result AccessPropertyFromContextReference(Category category, int position)
        {
            return AccessFromContextReference(Category.Type, position)
                .Type
                .Apply(category, TypeBase.VoidResult, RefAlignParam);
        }

        private Result AccessFromContextReference(Category category, int position)
        {
            return AccessFromThisReference(category, ThisReferenceFromContextReferenceResult(category | Category.Type), position);
        }

        internal Result AccessFromThisReference(Category category, int position)
        {
            Tracer.Assert(_position > position);
            return ContainerContextObject
                .SpawnAccessObject(position)
                .Access(category, this, position,false);
        }

        private Result AccessFromThisReference(Category category, Result thisReferenceResult, int position)
        {
            return AccessFromThisReference(category, position).ReplaceArg(thisReferenceResult);
        }

        private Result ContextReferenceFromThisReference(Category category) { return new Result(category, () => RefAlignParam.RefSize, ContextReferenceFromThisReferenceCode, Refs.None); }

        private CodeBase ThisReferenceFromContextReferenceCode()
        {
            return CodeBase
                .ReferenceInCode(ContainerContextObject)
                .AddToReference(RefAlignParam, StructSize*-1, "ContextCode");
        }

        private CodeBase ContextReferenceFromThisReferenceCode()
        {
            return CodeBase
                .Arg(RefAlignParam.RefSize)
                .AddToReference(RefAlignParam, StructSize, "ContextReferenceAsArg");
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            var reference = type.Reference(RefAlignParam);
            var result = reference
                .Result(category, () => CreateRefArgCode().AddToReference(RefAlignParam, offset, "AutomaticDereference"));
            return result.AutomaticDereference() & category;
        }

        private CodeBase CreateRefArgCode() { return Type.Reference(RefAlignParam).ArgCode(); }
    }
}