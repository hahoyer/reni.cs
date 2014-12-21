using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class Structure : DumpableObject
    {
        static int _nextObjectId;
        [EnableDump]
        [Node]
        internal ContainerContextObject ContainerContextObject;
        [EnableDump]
        [Node]
        internal int EndPosition;
        [Node]
        readonly ValueCache<StructureType> _typeCache;
        [Node]
        readonly FunctionCache<int, TypeBase> _accessTypesCache;
        [Node]
        readonly FunctionCache<int, AccessFeature> _accessFeaturesCache;
        [Node]
        readonly FunctionCache<FunctionSyntax, FunctionBodyType> _functionBodyTypeCache;

        internal Structure(ContainerContextObject containerContextObject, int endPosition)
            : base(_nextObjectId++)
        {
            _functionBodyTypeCache = new FunctionCache<FunctionSyntax, FunctionBodyType>
                (syntax => new FunctionBodyType(this, syntax));
            ContainerContextObject = containerContextObject;
            EndPosition = endPosition;
            _typeCache = new ValueCache<StructureType>(() => new StructureType(this));
            _accessTypesCache = new FunctionCache<int, TypeBase>(AccessTypeForCache);
            _accessFeaturesCache = new FunctionCache<int, AccessFeature>(position => new AccessFeature(this, position));
            StopByObjectId(-313);
        }

        [DisableDump]
        internal ContextBase UniqueContext
        {
            get
            {
                return ContainerContextObject
                    .Parent
                    .UniqueStructurePositionContext(ContainerContextObject.Container, EndPosition);
            }
        }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        internal TypeBase PointerKind { get { return Type.PointerKind; } }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump() + "(" + ContainerContextObject.NodeDump + "@" + EndPosition + ")";
        }

        [DisableDump]
        TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        bool _isObtainStructSizeActive;

        [DisableDump]
        internal Size StructSize
        {
            get
            {
                if(_isObtainStructSizeActive)
                    throw new RecursionWhileObtainingStructSizeException(this);

                try
                {
                    _isObtainStructSizeActive = true;
                    var result = ContainerContextObject.StructureSize(EndPosition);
                    _isObtainStructSizeActive = false;
                    return result;
                }
                catch(RecursionWhileObtainingStructSizeException)
                {
                    _isObtainStructSizeActive = false;
                    return null;
                }
            }
        }

        [DisableDump]
        internal bool Hllw { get { return ContainerContextObject.StructureHllw(EndPosition); } }

        [DisableDump]
        internal Root RootContext { get { return ContainerContextObject.RootContext; } }

        sealed class RecursionWhileObtainingStructSizeException : Exception
        {
            [EnableDump]
            readonly Structure _structure;
            public RecursionWhileObtainingStructSizeException(Structure structure) { _structure = structure; }
        }

        internal TypeBase UniqueFunctionalType(FunctionSyntax syntax) { return _functionBodyTypeCache[syntax]; }

        TypeBase UniqueAccessType(int position) { return _accessTypesCache[position]; }
        internal AccessFeature UniqueAccessFeature(int position) { return _accessFeaturesCache[position]; }

        TypeBase AccessTypeForCache(int position)
        {
            if(ContainerContextObject.Container.IsConst(position))
                return ContainerContextObject.AccessType(EndPosition, position);
            return new FieldAccessType(this, position);
        }

        internal Result AccessViaThisReference(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .SmartUn<PointerType>()
                .Evaluate(ContainerContextObject.RootContext.ExecutionContext)
                .ToInt32();
            return AccessViaThisReference(category, position);
        }

        internal Size FieldOffset(int position)
        {
            return ContainerContextObject.FieldOffsetFromAccessPoint(EndPosition, position);
        }

        internal Result DumpPrintResultViaStructReference(Category category)
        {
            return DumpPrintResultViaContextReference(category)
                .ContextReferenceViaStructReference(this);
        }

        internal Result DumpPrintResultViaContextReference(Category category)
        {
            return RootContext.ConcatPrintResult
                (
                    category,
                    EndPosition,
                    position => DumpPrintResultViaAccessReference(category, position));
        }

        internal Result AccessViaThisReference(Category category, int position)
        {
            var resultType = UniqueAccessType(position);
            if(resultType.Hllw)
                return resultType.Result(category);

            return resultType
                .Result(category, PointerKind.ArgResult(category));
        }

        internal FunctionType Function(FunctionSyntax body, TypeBase argsType)
        {
            return ContainerContextObject
                .RootContext
                .FunctionInstance(this, body, argsType);
        }

        internal Result AccessViaContextReference(Category category, int position)
        {
            var accessType = UniqueAccessType(position);
            var result = accessType
                .Result(category, ContainerContextObject)
                .AddToReference(ContextOffset);
            return result;
        }

        Size ContextOffset() { return ContainerContextObject.ContextReferenceOffsetFromAccessPoint(EndPosition) * -1; }

        internal Result StructReferenceViaContextReference(Category category)
        {
            if(Hllw)
                return Type.Result(category);

            return PointerKind
                .Result
                (
                    category,
                    StructReferenceCodeViaContextReference,
                    () => CodeArgs.Create(ContainerContextObject)
                );
        }

        Result DumpPrintResultViaAccessReference(Category category, int position)
        {
            var trace = position == -1;
            StartMethodDump(trace, category, position);
            try
            {
                var accessType = UniqueAccessType(position);
                var genericDumpPrintResult = accessType.GenericDumpPrintResult(category);
                Dump("genericDumpPrintResult", genericDumpPrintResult);
                BreakExecution();
                var accessViaThisReference = AccessViaThisReference(category.Typed, position);
                return ReturnMethodDump(genericDumpPrintResult.ReplaceArg(accessViaThisReference));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result ContextReferenceViaStructReference(Result result)
        {
            return ContainerContextObject
                .ContextReferenceViaStructReference(EndPosition, result);
        }

        CodeBase StructReferenceCodeViaContextReference()
        {
            return CodeBase.ReferenceCode(ContainerContextObject)
                .ReferencePlus(StructSize * -1);
        }

        internal TypeBase ValueType(int position)
        {
            return ContainerContextObject
                .AccessType(EndPosition, position)
                .TypeForStructureElement;
        }
    }
}