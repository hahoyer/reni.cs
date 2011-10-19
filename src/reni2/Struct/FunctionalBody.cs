// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionalBody : FunctionalFeature
    {
        readonly CompileSyntax _body;
        readonly Structure _structure;
        readonly SimpleCache<CallType> _typeCache;
        readonly SimpleCache<AutoCallType> _autoCallCache;

        internal FunctionalBody(Structure structure, CompileSyntax body)
        {
            _structure = structure;
            _body = body;
            _autoCallCache = new SimpleCache<AutoCallType>(() => new AutoCallType(this));
            _typeCache = new SimpleCache<CallType>(() => new CallType(this));
            StopByObjectId(-1);
        }

        internal abstract class Type : TypeBase
        {
            readonly FunctionalBody _parent;

            protected Type(FunctionalBody parent) { _parent = parent; }

            [DisableDump]
            internal override Structure FindRecentStructure { get { return _parent._structure; } }
            [DisableDump]
            protected abstract TypeBase ArgsType { get; }
            [DisableDump]
            internal override bool IsDataLess { get { return _parent.IsObjectForCallRequired; } }
            [DisableDump]
            protected abstract string Tag { get; }

            internal override string DumpPrintText { get { return _parent._body.DumpPrintText + Tag; } }
            internal override IFunctionalFeature FunctionalFeature { get { return _parent; } }

            protected override Size GetSize() { return _parent.RefAlignParam.RefSize; }
            protected Result ObtainApplyResult(Category category) { return _parent.ObtainApplyResult(category, ArgsType); }
        }

        internal sealed class CallType : Type
        {
            public CallType(FunctionalBody parent)
                : base(parent) { }

            protected override TypeBase ArgsType { get { return null; } }

            protected override Converter ConverterForUnalignedTypes(ConversionParameter conversionParameter, TypeBase destination)
            {
                var arrayDestination = destination as Reni.Type.Array;
                if(arrayDestination != null)
                    return new FunctionalConverter(category => arrayDestination.Result(category, FunctionalFeature));
                NotImplementedMethod(conversionParameter, destination);
                return null;
            }
            protected override string Tag { get { return "/\\"; } }
        }

        sealed class AutoCallType : Type
        {
            internal AutoCallType(FunctionalBody parent)
                : base(parent) { }


            Result ValueResult(Category category) { return ObtainApplyResult(category).ReplaceArg(Void.Result(category.Typed)); }

            internal override void Search(SearchVisitor searchVisitor)
            {
                base.Search(searchVisitor);
                searchVisitor.Search(ValueType, new ConversionFunction(this));
            }

            sealed class ConversionFunction : Reni.ConversionFunction
            {
                readonly AutoCallType _parent;
                public ConversionFunction(AutoCallType parent) { _parent = parent; }
                [DisableDump]
                internal override TypeBase ArgType { get { return _parent; } }
                internal override Result Result(Category category) { return _parent.ValueResult(category); }
            }

            [DisableDump]
            TypeBase ValueType { get { return ValueResult(Category.Type).Type; } }
            [DisableDump]
            protected override TypeBase ArgsType { get { return Void; } }
            [DisableDump]
            protected override string Tag { get { return "/!\\"; } }
        }

        [DisableDump]
        bool IsObjectForCallRequired { get { return _structure.IsObjectForCallRequired(Body); } }

        [DisableDump]
        CompileSyntax Body { get { return _body; } }

        [DisableDump]
        protected override TypeBase ObjectType { get { return _structure.Type; } }

        protected override Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam == RefAlignParam);
            return _structure.ContextReferenceViaStructReference(result);
        }

        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }

        internal override string DumpShort() { return base.DumpShort() + "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _structure.ObjectId + "#)#"; }

        protected override Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return ObtainApplyResult(category, argsType); }

        Result ObtainApplyResult(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -3 && category.HasCode;
            StartMethodDump(trace, category, argsType);
            try
            {
                var argsResult = argsType.ArgResult(category.Typed);
                var result = _structure.Call(category, Body, argsResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result Result(Category category, bool isAutoCall) { return UniqueType(isAutoCall).Result(category); }

        TypeBase UniqueType(bool isAutoCall)
        {
            if(isAutoCall)
                return _autoCallCache.Value;
            return _typeCache.Value;
        }
    }
}