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
    [Serializable]
    internal sealed class Type : TypeBase
    {
        [Node]
        internal readonly FullContext Context;

        internal Type(FullContext context) { Context = context; }

        protected override Size GetSize() { return Context.InternalSize(); }
        internal override string DumpShort() { return "type." + ObjectId + "(context." + Context.DumpShort() + ")"; }
        internal protected override int IndexSize { get { return Context.IndexSize; } }

        private List<ICompileSyntax> StatementList { get { return Context.StatementList; } }

        internal override Result AccessResultFromRef(Category category, int position,
            RefAlignParam refAlignParam) { return Context.AccessResultFromRef(category, position, refAlignParam); }

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

        internal override Result ConvertToVirt(Category category, TypeBase dest) { 
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            Tracer.Assert(category == Category.Refs);
            return new Result { Refs = Context.ConstructorRefs() };
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            if (dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest,conversionFeature);
            return false;
        }

        internal override SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable)
        {
            var containerResult = Context.Container.SearchFromRefToStruct(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature,
                this);
            if(result.IsSuccessFull)
                return result;
            return base.SearchFromRef(defineable).SubTrial(this);
        }
    }
}