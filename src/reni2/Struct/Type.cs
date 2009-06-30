using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
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

        private readonly SimpleCache<ArrayFeature> _arrayFeature;

        internal Type(FullContext context)
        {
            _arrayFeature = new SimpleCache<ArrayFeature>(() => new ArrayFeature(this));
            Context = context;
        }

        protected override Size GetSize() { return Context.InternalSize(); }
        internal override string DumpShort() { return "type." + ObjectId + "(context." + Context.DumpShort() + ")"; }
        internal protected override int IndexSize { get { return Context.IndexSize; } }

        private List<ICompileSyntax> StatementList { get { return Context.StatementList; } }
        internal IConverter<IFeature, Ref> ArrayFeature { get { return _arrayFeature.Value; } }

        internal override Result AccessResultAsArgFromRef(Category category, int position, RefAlignParam refAlignParam) { return Context.AccessResultAsArgFromRef(category, position, refAlignParam); }

        internal override Result AccessResultAsContextRefFromRef(Category category, int position,
                                                                 RefAlignParam refAlignParam) { return Context.AccessResultAsContextRefFromRef(category, position, refAlignParam); }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(Context.RefAlignParam));
            var result = new List<Result>();
            for(var i = 0; i < StatementList.Count; i++)
            {
                var accessResult = AccessResultAsArgFromRef(category | Category.Type, i, refAlignParam);
                result.Add(accessResult.Type.DumpPrint(category).UseWithArg(accessResult));
            }
            return Result.ConcatPrintResult(category, result);
        }

        internal override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.CreateResult(category,
                                     () => CodeBase.CreateArg(Size.Zero), () => Context.ConstructorRefs());
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionFeature);
            return false;
        }

        internal override SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable)
        {
            var containerResult = Context.Container.SearchFromRefToStruct(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature,
                                                                        this);
            if(result.IsSuccessFull)
                return result;
            return base.SearchFromRef(defineable).SubTrial(this, "try at base class");
        }
    }

    internal class ArrayFeature : ReniObject, IConverter<IFeature, Ref>, IFeature
    {
        private readonly Type _type;

        public ArrayFeature(Type type) { _type = type; }
        public IFeature Convert(Ref type) { return this; }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                    ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);
            return callContext.ApplyResult(category, @object,
                                           typeBase =>
                                           (_type.Context.Container.ArrayConversion(category)).UseWithArg(typeBase.ConvertTo(category,
                                                                                                            _type)));
        }
    }
}