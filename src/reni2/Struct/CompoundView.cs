using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;


namespace Reni.Struct
{
    sealed class CompoundView : DumpableObject
    {
        static int _nextObjectId;
        [EnableDump]
        [Node]
        internal Compound Compound;
        [EnableDump]
        [Node]
        internal int ViewPosition;
        [Node]
        readonly ValueCache<CompoundType> _typeCache;
        [Node]
        readonly FunctionCache<int, TypeBase> _accessTypesCache;
        [Node]
        readonly FunctionCache<int, AccessFeature> _accessFeaturesCache;
        [Node]
        readonly FunctionCache<int, FieldAccessType> _fieldAccessTypeCache;
        [Node]
        readonly FunctionCache<FunctionSyntax, FunctionBodyType> _functionBodyTypeCache;

        internal CompoundView(Compound compound, int viewPosition)
            : base(_nextObjectId++)
        {
            _fieldAccessTypeCache =
                new FunctionCache<int, FieldAccessType>(position => new FieldAccessType(this, position));
            _functionBodyTypeCache = new FunctionCache<FunctionSyntax, FunctionBodyType>
                (syntax => new FunctionBodyType(this, syntax));
            Compound = compound;
            ViewPosition = viewPosition;
            _typeCache = new ValueCache<CompoundType>(() => new CompoundType(this));
            _accessTypesCache = new FunctionCache<int, TypeBase>(AccessType);
            _accessFeaturesCache = new FunctionCache<int, AccessFeature>(position => new AccessFeature(this, position));
            StopByObjectId(-313);
        }

        [DisableDump]
        internal ContextBase UniqueContext => Compound.Parent
            .UniqueStructurePositionContext(Compound.Syntax, ViewPosition);

        [DisableDump]
        internal CompoundType Type => _typeCache.Value;

        [DisableDump]
        internal TypeBase PointerKind => Type.PointerKind;

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Compound.NodeDump + "@" + ViewPosition + ")";

        [DisableDump]
        TypeBase IndexType => Compound.IndexType;

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
                    var result = Compound.StructureSize(ViewPosition);
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
        internal bool Hllw => Compound.StructureHllw(ViewPosition);

        [DisableDump]
        internal Root RootContext => Compound.RootContext;

        sealed class RecursionWhileObtainingStructSizeException : Exception
        {
            [EnableDump]
            readonly CompoundView _compoundView;

            public RecursionWhileObtainingStructSizeException(CompoundView compoundView) { _compoundView = compoundView; }
        }

        internal TypeBase UniqueFunctionalType(FunctionSyntax syntax) => _functionBodyTypeCache[syntax];

        internal AccessFeature UniqueAccessFeature(int position) => _accessFeaturesCache[position];

        TypeBase AccessType(int position)
        {
            var result = Compound.AccessType(ViewPosition, position);
            if(result.Hllw)
                return result;
            return _fieldAccessTypeCache[position];
        }

        internal Result AccessViaThisReference(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .SmartUn<PointerType>()
                .Evaluate(Compound.RootContext.ExecutionContext)
                .ToInt32();
            return AccessViaThisReference(category, position);
        }

        internal Size FieldOffset(int position) => Compound.FieldOffsetFromAccessPoint(ViewPosition, position);

        internal Result DumpPrintResultViaStructReference(Category category) => DumpPrintResultViaContextReference(category)
            .ContextReferenceViaStructReference(this);

        internal Result DumpPrintResultViaContextReference(Category category)
            => RootContext.ConcatPrintResult
                (
                    category,
                    ViewPosition,
                    position => DumpPrintResultViaAccessReference(category, position)
                );

        internal Result AccessViaThisReference(Category category, int position)
        {
            var resultType = AccessType(position);
            if(resultType.Hllw)
                return resultType.Result(category);

            return resultType
                .Result(category, PointerKind.ArgResult(category));
        }

        internal FunctionType Function(FunctionSyntax body, TypeBase argsType) => Compound
            .RootContext
            .FunctionInstance(this, body, argsType);

        internal Result AccessViaContextReference(Category category, int position)
        {
            var accessType = AccessType(position);
            var result = accessType
                .Result(category, Compound)
                .AddToReference(ContextOffset);
            return result;
        }

        Size ContextOffset() => Compound.ContextReferenceOffsetFromAccessPoint(ViewPosition) * -1;

        internal Result StructReferenceViaContextReference(Category category)
        {
            if(Hllw)
                return Type.Result(category);

            return PointerKind
                .Result
                (
                    category,
                    StructReferenceCodeViaContextReference,
                    () => CodeArgs.Create(Compound)
                );
        }

        Result DumpPrintResultViaAccessReference(Category category, int position)
        {
            var trace = position == -10;
            StartMethodDump(trace, category, position);
            try
            {
                var accessType = AccessType(position);
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

        internal Result ContextReferenceViaStructReference(Result result) => Compound
            .ContextReferenceViaStructReference(ViewPosition, result);

        CodeBase StructReferenceCodeViaContextReference() => CodeBase.ReferenceCode(Compound)
            .ReferencePlus(StructSize * -1);

        internal TypeBase ValueType(int position) => Compound
            .AccessType(ViewPosition, position)
            .TypeForStructureElement;

        internal IFeatureImplementation Find(Definable definable)
            => Compound.Syntax.Find(definable.Name)?.Convert(this);
    }
}