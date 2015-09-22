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
        internal readonly Compound Compound;
        [EnableDump]
        [Node]
        internal readonly int ViewPosition;
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
            Compound = compound;
            ViewPosition = viewPosition;

            _typeCache = new ValueCache<CompoundType>(() => new CompoundType(this));

            _accessFeaturesCache
                = new FunctionCache<int, AccessFeature>
                    (position => new AccessFeature(this, position));

            _fieldAccessTypeCache
                = new FunctionCache<int, FieldAccessType>
                    (position => new FieldAccessType(this, position));

            _functionBodyTypeCache
                = new FunctionCache<FunctionSyntax, FunctionBodyType>
                    (syntax => new FunctionBodyType(this, syntax));

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
        internal IEnumerable<IConversion> ConverterFeatures
            => Compound
                .Syntax
                .ConverterFunctions
                .Select(ConversionFunction);

        public string DumpPrintTextOfType
            => Compound
                .Syntax
                .EndPosition
                .Select(position => Compound.AccessType(ViewPosition, position).DumpPrintText)
                .Stringify(", ")
                .Surround("(", ")");

        public IEnumerable<IConversion> MixinConversions
            => Compound
                .Syntax
                .MixIns
                .Select(AccessFeature);

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

        internal TypeBase AccessType(int position)
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

            return Type.SmartPointer.Mutation(resultType) & category;
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

        IConversion ConversionFunction(FunctionSyntax body)
        {
            IConversion result = new ConverterAccess(Function(body, RootContext.VoidType), Type);
            var source = result.Source;
            Tracer.Assert(source == Type.Pointer, source.Dump);
            Tracer.Assert(source == result.Result(Category.Code).Code.ArgType);
            return result;
        }

        sealed class ConverterAccess : DumpableObject, IConversion
        {
            [EnableDump]
            readonly FunctionType Parent;
            [EnableDump]
            readonly CompoundType Type;

            public ConverterAccess(FunctionType parent, CompoundType type)
            {
                Parent = parent;
                Type = type;
            }

            Result IConversion.Execute(Category category)
            {
                var innerResult = ((IConversion) Parent).Execute(category);
                var conversion = Type.Pointer.Mutation(Parent);
                var result = innerResult.ReplaceArg(conversion);
                return result;
            }

            TypeBase IConversion.Source => Type.Pointer;
        }

        internal FunctionType Function(FunctionSyntax body, TypeBase argsType)
            => Compound
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

        CodeBase ObjectPointerViaContext()
            => CodeBase.ReferenceCode(Compound).ReferencePlus(CompoundViewSize * -1);

        internal TypeBase ValueType(int position)
            => Compound
                .AccessType(ViewPosition, position)
                .TypeForStructureElement;

        internal IImplementation Find(Definable definable)
            => Compound.Syntax.Find(definable, this);
    }
}