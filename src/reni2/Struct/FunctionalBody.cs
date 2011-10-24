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
        internal readonly Structure Structure;
        readonly SimpleCache<CallType> _callCache;
        readonly SimpleCache<AutoCallType> _autoCallCache;

        internal FunctionalBody(Structure structure, CompileSyntax body)
        {
            Structure = structure;
            _body = body;
            _autoCallCache = new SimpleCache<AutoCallType>(() => new AutoCallType(this));
            _callCache = new SimpleCache<CallType>(() => new CallType(this));
            StopByObjectId(-1);
        }

        [DisableDump]
        internal bool IsObjectForCallRequired { get { return !Structure.IsDataLess && Structure.IsObjectForCallRequired(Body); } }

        [DisableDump]
        internal CompileSyntax Body { get { return _body; } }

        [DisableDump]
        protected override TypeBase ObjectType { get { return Structure.Type; } }

        protected override Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam == RefAlignParam);
            return Structure.ContextReferenceViaStructReference(result);
        }

        internal RefAlignParam RefAlignParam { get { return Structure.RefAlignParam; } }

        internal override string DumpShort() { return base.DumpShort() + "(" + _body.DumpShort() + ")/\\" + "#(#in context." + Structure.ObjectId + "#)#"; }

        protected override Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return ObtainApplyResult(category, argsType); }

        internal Result ObtainApplyResult(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -3 && category.HasCode;
            StartMethodDump(trace, category, argsType);
            try
            {
                var argsResult = argsType.ArgResult(category.Typed);
                var result = Structure.Call(category, Body, argsResult);
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
            return _callCache.Value;
        }
    }
}