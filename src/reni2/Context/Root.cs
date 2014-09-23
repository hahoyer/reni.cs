using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class Root : ContextBase
    {
        [DisableDump]
        [Node]
        readonly FunctionList _functions = new FunctionList();
        [DisableDump]
        [Node]
        internal readonly IExecutionContext ExecutionContext;

        readonly ValueCache<BitType> _bitCache;
        readonly ValueCache<VoidType> _voidCache;

        internal Root(IExecutionContext executionContext)
        {
            ExecutionContext = executionContext;
            _bitCache = new ValueCache<BitType>(() => new BitType(this));
            _voidCache = new ValueCache<VoidType>(() => new VoidType(this));
        }

        [DisableDump]
        internal override Root RootContext { get { return this; } }

        [DisableDump]
        [Node]
        internal BitType BitType { get { return _bitCache.Value; } }

        [DisableDump]
        [Node]
        internal VoidType VoidType { get { return _voidCache.Value; } }

        [DisableDump]
        internal int FunctionCount { get { return _functions.Count; } }

        internal static RefAlignParam DefaultRefAlignParam
        {
            get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); }
        }

        Result ConcatArraysResult(Category category, IContextReference context, TypeBase argsType)
        {
            NotImplementedMethod(category, context, argsType);
            return null;
        }

        internal FunctionType FunctionInstance(Structure structure, FunctionSyntax body, TypeBase argsType)
        {
            var alignedArgsType = argsType.UniqueAlign;
            var functionInstance = _functions.Find(body, structure, alignedArgsType);
            return functionInstance;
        }
        internal Result VoidResult(Category category) { return VoidType.Result(category); }

        internal Result ConcatPrintResult(Category category, int count, Func<int, Result> elemResults)
        {
            var result = VoidResult(category);
            if(!(category.HasCode || category.HasExts))
                return result;

            if(category.HasCode)
                result.Code = CodeBase.DumpPrintText("(");

            for(var i = 0; i < count; i++)
            {
                var elemResult = elemResults(i);
                result.IsDirty = true;
                if(category.HasCode)
                {
                    if(i > 0)
                        result.Code = result.Code + CodeBase.DumpPrintText(", ");
                    result.Code = result.Code + elemResult.Code;
                }
                if(category.HasExts)
                    result.Exts = result.Exts.Sequence(elemResult.Exts);
                result.IsDirty = false;
            }
            if(category.HasCode)
                result.Code = result.Code + CodeBase.DumpPrintText(")");
            return result;
        }

        internal FunctionContainer FunctionContainer(int index) { return _functions.Container(index); }

        internal Code.Container MainContainer(ParsedSyntax syntax, string description)
        {
            return Struct.Container
                .Create(syntax)
                .Code(this)
                .Container(description);
        }
    }
}