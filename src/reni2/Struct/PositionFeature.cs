using HWClassLibrary.Debug;
using System;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class PositionFeatureBase : ReniObject, IContextFeature, IConverter<IFeature, Ref>, IFeature
    {
        private readonly IStructContext _structContext;

        [DumpData(true)]
        private readonly int _index;

        protected PositionFeatureBase(IStructContext structContext, int index)
        {
            _index = index;
            _structContext = structContext;
        }

        IFeature IConverter<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == _structContext.ForCode.RefAlignParam);
            return this;
        }

        [DumpData(false)]
        internal Ref NaturalRefType { get { return _structContext.NaturalRefType; } }

        public abstract Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax args);

        public abstract Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                           ICompileSyntax args);

        protected Result ApplyResult(ContextBase callContext, Category category, Result objectResult,
                                     ICompileSyntax args)
        {
            var trace = ObjectId == 303 && callContext.ObjectId == 10 && (category.HasRefs || category.HasRefs);
            StartMethodDumpWithBreak(trace, callContext, category, objectResult, args);
            var accessResult = NaturalRefType.AccessResult(category | Category.Type, _index).UseWithArg(objectResult);
            if(args == null)
                return ReturnMethodDumpWithBreak(trace, accessResult);
            Dump(trace, "accessResult", accessResult);
            var result = accessResult.Type.ApplyFunction(category, callContext, args);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        protected Result NaturalResult(Category category)
        {
            return NaturalRefType.CreateContextResult(_structContext.ForCode, category | Category.Type);
        }

        protected Result ObjectResult(ContextBase callContext, Category category, ICompileSyntax @object)
        {
            return callContext.ResultAsRef(category | Category.Type, @object).ConvertTo(NaturalRefType);
        }

    }

    [Serializable]
    internal sealed class PositionFeature : PositionFeatureBase
    {
        private readonly PropertyPositionFeature _property;

        internal PositionFeature(EmptyList emptyList, IStructContext structContext, int index)
            : base(structContext, index)
        {
            _property = new PropertyPositionFeature(emptyList, structContext, index);
        }

        public override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                           ICompileSyntax args)
        {
            return ApplyResult(callContext, category, ObjectResult(callContext, category, @object), args);
        }

        public override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax args) { return ApplyResult(callContext, category, NaturalResult(category), args); }

        public PositionFeatureBase ToProperty(bool isPoperty)
        {
            if(isPoperty)
                return _property;
            return this;
        }
    }

    internal class PropertyPositionFeature : PositionFeatureBase
    {
        private readonly EmptyList _emptyList;

        public PropertyPositionFeature(EmptyList emptyList, IStructContext structContext, int index)
            : base(structContext, index)
        {
            _emptyList = emptyList;
        }

        public override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax args)
        {
            if(args == null)
                return ApplyResult(callContext, category, NaturalResult(category), _emptyList);
            NotImplementedMethod(callContext, category, args);
            return null;
        }

        public override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                           ICompileSyntax args)
        {
            if (args == null)
                return ApplyResult(callContext, category, ObjectResult(callContext, category, @object), _emptyList);
            NotImplementedMethod(callContext, category, @object, args);
            return null;
        }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
    }
}