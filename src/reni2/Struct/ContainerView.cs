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
    sealed class ContainerView : DumpableObject
    {
        static int _nextObjectId;
        [EnableDump]
        [Node]
        internal Container Container;
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

        internal ContainerView(Container container, int endPosition)
            : base(_nextObjectId++)
        {
            _functionBodyTypeCache = new FunctionCache<FunctionSyntax, FunctionBodyType>
                (syntax => new FunctionBodyType(this, syntax));
            Container = container;
            EndPosition = endPosition;
            _typeCache = new ValueCache<StructureType>(() => new StructureType(this));
            _accessTypesCache = new FunctionCache<int, TypeBase>(position1 => AccessTypeForCache(this, position1, this.Container, this.EndPosition));
            _accessFeaturesCache = new FunctionCache<int, AccessFeature>(position => new AccessFeature(this, position));
            StopByObjectId(-313);
        }

        [DisableDump]
        internal ContextBase UniqueContext
        {
            get
            {
                return Container
                    .Parent
                    .UniqueStructurePositionContext(Container.Syntax, EndPosition);
            }
        }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        internal TypeBase PointerKind { get { return Type.PointerKind; } }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump() + "(" + Container.NodeDump + "@" + EndPosition + ")";
        }

        [DisableDump]
        TypeBase IndexType { get { return Container.IndexType; } }

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
                    var result = Container.StructureSize(EndPosition);
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
        internal bool Hllw { get { return Container.StructureHllw(EndPosition); } }

        [DisableDump]
        internal Root RootContext { get { return Container.RootContext; } }

        sealed class RecursionWhileObtainingStructSizeException : Exception
        {
            [EnableDump]
            readonly ContainerView _containerView;
            public RecursionWhileObtainingStructSizeException(ContainerView containerView) { _containerView = containerView; }
        }

        internal TypeBase UniqueFunctionalType(FunctionSyntax syntax) { return _functionBodyTypeCache[syntax]; }

        TypeBase UniqueAccessType(int position) { return _accessTypesCache[position]; }
        internal AccessFeature UniqueAccessFeature(int position) { return _accessFeaturesCache[position]; }

        static TypeBase AccessTypeForCache(ContainerView containerView, int position, Container container, int endPosition)
        {
            if(container.Syntax.IsConst(position))
                return container.AccessType(endPosition, position);
            return new FieldAccessType(containerView, position);
        }

        internal Result AccessViaThisReference(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .SmartUn<PointerType>()
                .Evaluate(Container.RootContext.ExecutionContext)
                .ToInt32();
            return AccessViaThisReference(category, position);
        }

        internal Size FieldOffset(int position)
        {
            return Container.FieldOffsetFromAccessPoint(EndPosition, position);
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
            var resultType = AccessTypeForCache(this, position, this.Container, this.EndPosition);
            if(resultType.Hllw)
                return resultType.Result(category);

            return resultType
                .Result(category, PointerKind.ArgResult(category));
        }

        internal FunctionType Function(FunctionSyntax body, TypeBase argsType)
        {
            return Container
                .RootContext
                .FunctionInstance(this, body, argsType);
        }

        internal Result AccessViaContextReference(Category category, int position)
        {
            var accessType = UniqueAccessType(position);
            var result = accessType
                .Result(category, Container)
                .AddToReference(ContextOffset);
            return result;
        }

        Size ContextOffset() { return Container.ContextReferenceOffsetFromAccessPoint(EndPosition) * -1; }

        internal Result StructReferenceViaContextReference(Category category)
        {
            if(Hllw)
                return Type.Result(category);

            return PointerKind
                .Result
                (
                    category,
                    StructReferenceCodeViaContextReference,
                    () => CodeArgs.Create(Container)
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
            return Container
                .ContextReferenceViaStructReference(EndPosition, result);
        }

        CodeBase StructReferenceCodeViaContextReference()
        {
            return CodeBase.ReferenceCode(Container)
                .ReferencePlus(StructSize * -1);
        }

        internal TypeBase ValueType(int position)
        {
            return Container
                .AccessType(EndPosition, position)
                .TypeForStructureElement;
        }
    }
}