using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    [Obsolete("", true)]
    internal abstract class OldContext : Reni.Context.Child
    {
        private Size[] _offsetsCache;
        private TypeBase[] _typesCache;
        private bool _isFunctionInternalSizeActive;

        [Node]
        [IsDumpEnabled(false)]
        private Result _internalConstructorResult;

        [Node]
        [IsDumpEnabled(false)]
        private Result _constructorResult;

        protected OldContext(Container container)
        {
            _internalConstructorResult = new Result();
            _constructorResult = new Result();
        }

        [IsDumpEnabled(false)]
        internal abstract IReferenceInCode ForCode { get; }

        [IsDumpEnabled(false)]
        protected abstract int Position { get; }

        [IsDumpEnabled(false)]
        internal ICompileSyntax[] StatementList { get { return Container.List; } }

        internal abstract ContextAtPosition CreatePosition(int position);

        protected override string DumpShort() { return "context." + ObjectId + "(" + Container.DumpShort() + ")"; }

        internal TypeBase RawType(int position) { return InternalType(position); }

        private TypeBase InternalType(int position) { return InternalResult(Category.Type, position).Type; }

        internal Size Offset(int position) { return InternalResult(Category.Size, position + 1, Position).Size; }

        [IsDumpEnabled(false)]
        internal IEnumerable<TypeBase> Types { get { return _typesCache ?? (_typesCache = GetTypes().ToArray()); } }

        [IsDumpEnabled(false)]
        private Size[] Offsets { get { return _offsetsCache ?? (_offsetsCache = GetOffsets().ToArray()); } }

        internal ContextBase ToContext
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        private IEnumerable<Size> GetOffsets()
        {
            var sizes = Types.Select(typeBase => typeBase.Size).ToArray();
            return sizes.Aggregate(new Size[0], AggregateSizes);
        }

        private Size[] AggregateSizes(Size[] sizesSoFar, Size nextSize)
        {
            var result = sizesSoFar
                .Select(size => size + nextSize.Align(AlignBits))
                .Concat(new[] {Size.Zero})
                .ToArray<>();
            return result;
        }

        private IEnumerable<TypeBase> GetTypes() { return StatementList.Select(Type); }

        internal override sealed OldContext FindStruct() { return this; }

        private FunctionalFeatureType Function(ICompileSyntax body) { return _function.Find(body); }

        internal Result[] CreateArgCodes(Category category)
        {
            return Types
                .Select((type, i) => AutomaticDereference(type, Offsets[i], category))
                .ToArray();
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            var reference = type.Reference(RefAlignParam);
            var result = reference
                .Result(category, () => CreateRefArgCode().AddToReference(RefAlignParam, offset, "AutomaticDereference"));
            return result.AutomaticDereference() & category;
        }

        private CodeBase CreateRefArgCode() { return ContextType.Reference(RefAlignParam).ArgCode(); }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }

        internal Result PositionFeatureApply(Category category, int position, bool isProperty, bool isContextFeature)
        {
            var trace = position == -1 && category.HasCode && isContextFeature;
            StartMethodDump(trace, category, position, isProperty, isContextFeature);
            var accessResult = AccessResult(isProperty, position, category);
            var result = accessResult;
            if(!isContextFeature)
                result = result.ReplaceContextReferenceByArg(this);
            Dump(trace, "accessCode", accessResult.Code, "result.Code", result.Code);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        internal Result CreateFunctionCall(Category category, ICompileSyntax body, Result argsResult)
        {
            NotImplementedMethod(category, body, argsResult);
            return null;
        }
    }
}