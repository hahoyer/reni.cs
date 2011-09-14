//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
    internal sealed class FunctionalBody : FunctionalFeature
    {
        private readonly CompileSyntax _body;
        private readonly Structure _structure;
        private readonly SimpleCache<Type> _typeCache;

        internal FunctionalBody(Structure structure, CompileSyntax body)
        {
            _structure = structure;
            _body = body;
            _typeCache = new SimpleCache<Type>(() => new Type(this));
            StopByObjectId(-1);
        }

        internal sealed class Type : TypeBase
        {
            private readonly FunctionalBody _parent;
            public Type(FunctionalBody parent) { _parent = parent; }

            [DisableDump]
            internal override Structure FindRecentStructure { get { return _parent._structure; } }

            internal override Result PropertyResult(Category category)
            {
                return _parent
                    .ObtainApplyResult(category, Void)
                    .ReplaceArg(Void.Result(category.Typed));
            }

            internal override Size GetSize(bool isQuick) { return Size.Zero; }
            internal override string DumpPrintText { get { return _parent._body.DumpPrintText + "/\\"; } }
            internal override IFunctionalFeature FunctionalFeature { get { return _parent; } }
            protected override Converter ConverterForUnalignedTypes(ConversionParameter conversionParameter, TypeBase destination)
            {
                var arrayDestination = destination as Reni.Type.Array;
                if(arrayDestination != null)
                    return new FunctionalConverter(category => arrayDestination.Result(category, FunctionalFeature));
                NotImplementedMethod(conversionParameter, destination);
                return null;
            }
        }

        [DisableDump]
        private CompileSyntax Body { get { return _body; } }

        [DisableDump]
        protected override TypeBase ObjectType { get { return _structure.Type; } }

        protected override Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam == _structure.RefAlignParam);
            return _structure.ContextReferenceViaStructReference(result);
        }

        internal override string DumpShort() { return base.DumpShort() + "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _structure.ObjectId + "#)#"; }

        protected override Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return ObtainApplyResult(category, argsType); }

        private Result ObtainApplyResult(Category category, TypeBase argsType)
        {
            StartMethodDump(false, category, argsType);
            try
            {
                var argsResult = argsType.ArgResult(category.Typed);
                var result = _structure.CreateFunctionCall(category, Body, argsResult);
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

        internal Result Result(Category category) { return ToType().Result(category); }
        private Type ToType() { return _typeCache.Value; }
    }
}