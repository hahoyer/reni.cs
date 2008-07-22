using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Type : TypeBase
    {
        [Node]
        internal readonly Context Context;
        [Node]
        private Result _internalResult = new Result();
        [Node]
        private Result _constructorResult = new Result();

        public Type(Context context)
        {
            Context = context;
        }

        internal override string DumpShort()
        {
            return "type." + ObjectId + "(context." + Context.ObjectId + ")";
        }

        [DumpData(false)]
        internal protected override int IndexSize { get { return Context.IndexSize; } }

        protected override Size GetSize()
        {
            return InternalResult(Category.Size).Size;
        }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalResult.Update(internalResult);
            var constructorResult = CreateResult(category, internalResult);
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(Context.RefAlignParam));
            var result = new List<Result>();
            for(var i = 0; i < StatementList.Count; i++)
            {
                var accessResult = AccessResultFromRef(category | Category.Type, i, refAlignParam);
                result.Add(accessResult.Type.DumpPrint(category).UseWithArg(accessResult));
            }
            return Result.ConcatPrintResult(category, result);
        }

        private List<ICompileSyntax> StatementList { get { return Context.StatementList; } }

        private Result InternalResult(Category category)
        {
            return Context.InternalResult(category);
        }

        internal override SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable)
        {
            var containerResult = Context.Container.SearchFromRefToStruct(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature, this);
            if(result.IsSuccessFull)
                return result;
            return base.SearchFromRef(defineable).SubTrial(this);
        }

        internal override Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Context.AccessResultFromRef(category, position, refAlignParam);
        }

        internal sealed class Feature : ReniObject, IConverter<IFeature, Ref>, IFeature
        {
            [DumpData(true)]
            private readonly Type _type;
            [DumpData(true)]
            private readonly int _index;

            public Feature(Type type, int index)
            {
                _type = type;
                _index = index;
            }

            public IFeature Convert(Ref type)
            {
                Tracer.Assert(type.RefAlignParam == _type.Context.RefAlignParam);
                return this;
            }

            public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
            {
                var objectResult = callContext.ResultAsRef(category | Category.Type, @object);
                var accessResult = objectResult.Type.AccessResult(category, _index).UseWithArg(objectResult);
                if(args == null)
                    return accessResult;
                NotImplementedMethod(callContext, category, @object, args, "objectResult", objectResult, "accessResult", accessResult);
                return null;
            }
        }
    }
}