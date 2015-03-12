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
        readonly FunctionCache<int, AccessFeature> _accessFeaturesCache;
        [Node]
        readonly FunctionCache<int, FieldAccessType> _fieldAccessTypeCache;
        [Node]
        readonly FunctionCache<FunctionSyntax, FunctionBodyType> _functionBodyTypeCache;

        internal CompoundView(Compound compound, int viewPosition)
            : base(_nextObjectId++)
        {
            _fieldAccessTypeCache =
                new FunctionCache<int, FieldAccessType>
                    (position => new FieldAccessType(this, position));
            _functionBodyTypeCache = new FunctionCache<FunctionSyntax, FunctionBodyType>
                (syntax => new FunctionBodyType(this, syntax));
            Compound = compound;
            ViewPosition = viewPosition;
            _typeCache = new ValueCache<CompoundType>(() => new CompoundType(this));
            _accessFeaturesCache = new FunctionCache<int, AccessFeature>
                (position => new AccessFeature(this, position));
            StopByObjectId(-313);
        }

        public string GetCompoundIdentificationDump()
            => Compound.GetCompoundIdentificationDump() + "@" + ViewPosition;

        [DisableDump]
        internal ContextBase Context
            => Compound.Parent.CompoundPositionContext(Compound.Syntax, ViewPosition);

        [DisableDump]
        internal CompoundType Type => _typeCache.Value;

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

        [DisableDump]
        TypeBase IndexType => Compound.IndexType;

        bool _isObtainCompoundSizeActive;

        [DisableDump]
        internal Size CompoundViewSize
        {
            get
            {
                if(_isObtainCompoundSizeActive)
                    throw new RecursionWhileObtainingCompoundSizeException(this);

                try
                {
                    _isObtainCompoundSizeActive = true;
                    var result = Compound.Size(ViewPosition);
                    _isObtainCompoundSizeActive = false;
                    return result;
                }
                catch(RecursionWhileObtainingCompoundSizeException)
                {
                    _isObtainCompoundSizeActive = false;
                    return null;
                }
            }
        }

        [DisableDump]
        internal bool Hllw => Compound.Hllw(ViewPosition);

        [DisableDump]
        internal Root RootContext => Compound.RootContext;

        [DisableDump]
        internal IEnumerable<ISimpleFeature> ConverterFeatures
            => Compound
                .Syntax
                .Converters
                .Select(body => Function(body, RootContext.VoidType));

        sealed class RecursionWhileObtainingCompoundSizeException : Exception
        {
            [EnableDump]
            readonly CompoundView _compoundView;

            public RecursionWhileObtainingCompoundSizeException(CompoundView compoundView)
            {
                _compoundView = compoundView;
            }
        }

        internal TypeBase FunctionalType(FunctionSyntax syntax) => _functionBodyTypeCache[syntax];

        internal AccessFeature AccessFeature(int position) => _accessFeaturesCache[position];

        TypeBase AccessType(int position)
        {
            var result = Compound.AccessType(ViewPosition, position);
            if(result.Hllw)
                return result;
            return _fieldAccessTypeCache[position];
        }

        internal Result AccessViaObjectPointer(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .SmartUn<PointerType>()
                .Evaluate(Compound.RootContext.ExecutionContext)
                .ToInt32();
            return AccessViaObjectPointer(category, position);
        }

        internal Size FieldOffset(int position)
            => Compound.FieldOffsetFromAccessPoint(ViewPosition, position);

        internal Result DumpPrintResultViaObject(Category category)
            => RootContext.ConcatPrintResult
                (
                    category,
                    ViewPosition,
                    DumpPrintResultViaObject
                );

        Result AccessViaObjectPointer(Category category, int position)
        {
            var resultType = AccessType(position);
            if(resultType.Hllw)
                return resultType.Result(category);

            return resultType
                .Result(category, Type.SmartPointer.ArgResult(category));
        }

        internal Result AccessViaObject(Category category, int position)
        {
            var resultType = AccessType(position);
            if(resultType.Hllw)
                return resultType.Result(category);

            return resultType.Result(category, Type.ObjectResult);
        }

        Result AccessValueViaObject(Category category, int position)
        {
            var resultType = ValueType(position);
            if(resultType.Hllw)
                return resultType.Result(category);

            return resultType.Result
                (category, c => Type.ObjectResult(c).AddToReference(() => FieldOffset(position)));
        }


        internal Result ReplaceObjectPointerByContext(Result target)
        {
            var reference = Type.SmartPointer.CheckedReference;
            return target.ReplaceAbsolute(reference, ObjectPointerViaContext);
        }


        internal FunctionType Function(FunctionSyntax body, TypeBase argsType) => Compound
            .RootContext
            .FunctionInstance(this, body, argsType);

        internal Result ObjectPointerViaContext(Category category)
        {
            if(Hllw)
                return Type.Result(category);

            return Type.SmartPointer
                .Result
                (
                    category,
                    ObjectPointerViaContext,
                    () => CodeArgs.Create(Compound)
                );
        }

        Result DumpPrintResultViaObject(Category category, int position)
        {
            var trace = ObjectId == -10 && position == 0;
            StartMethodDump(trace, category, position);
            try
            {
                var accessType = ValueType(position).SmartPointer;
                var genericDumpPrintResult = accessType.GenericDumpPrintResult(category);
                Dump("genericDumpPrintResult", genericDumpPrintResult);
                BreakExecution();
                return ReturnMethodDump
                    (
                        genericDumpPrintResult.ReplaceAbsolute
                            (accessType.CheckedReference, c => AccessValueViaObject(c, position)));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result ContextViaObjectPointer(Result result)
            => Compound.ContextViaObjectPointer(ViewPosition, result);

        CodeBase ObjectPointerViaContext()
            => CodeBase.ReferenceCode(Compound).ReferencePlus(CompoundViewSize * -1);

        internal TypeBase ValueType(int position)
            => Compound
                .AccessType(ViewPosition, position)
                .TypeForStructureElement;

        internal IFeatureImplementation Find(Definable definable)
            => Compound.Syntax.Find(definable.Id)?.Convert(this);
    }
}