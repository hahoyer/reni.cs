#region Copyright (C) 2012

// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionalBodyType : TypeBase, IFunctionalFeature, IReferenceInCode
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly FunctionSyntax _syntax;
        readonly DictionaryEx<TypeBase, FunctionAccessType> _functionAccessTypesCache;

        public FunctionalBodyType(Structure structure, FunctionSyntax syntax)
        {
            _structure = structure;
            _syntax = syntax;
            _functionAccessTypesCache = new DictionaryEx<TypeBase, FunctionAccessType>(argsType => new FunctionAccessType(this, argsType));
        }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return _structure; } }
        [DisableDump]
        internal override bool IsLambda { get { return true; } }
        [DisableDump]
        internal override bool IsDataLess { get { return _structure.IsDataLess; } }
        [DisableDump]
        internal override bool IsLikeReference { get { return true; } }
        [DisableDump]
        RefAlignParam IReferenceInCode.RefAlignParam { get { return _structure.RefAlignParam; } }
        internal override string DumpPrintText { get { return _syntax.DumpPrintText; } }
        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return this; } }

        protected override Size GetSize() { return _structure.RefAlignParam.RefSize; }

        [DisableDump]
        bool IFunctionalFeature.IsImplicit { get { return _syntax.IsImplicit; } }
        [DisableDump]
        IReferenceInCode IFunctionalFeature.ObjectReference { get { return this; } }

        internal CodeArgs GetCodeArgs(TypeBase argsType) { return _structure.Function(_syntax, argsType).CodeArgs - CodeArgs.Arg(); }

        Result IFunctionalFeature.ApplyResult(Category category, TypeBase argsType)
        {
            return _functionAccessTypesCache
                .Find(argsType)
                .ApplyResult(category);
        }

        internal TypeBase ValueType(TypeBase argsType) { return _structure.Function(_syntax, argsType).ValueType; }

        Result CallResult(Category category, bool isGetter, TypeBase argsType)
        {
            var instance = _structure.Function(_syntax, argsType).Instance(isGetter);
            var result = instance.CallResult(category);
            if(category.HasArgs)
                result.CodeArgs = CodeArgs.Arg();
            if(category.HasCode)
                result.Code = _functionAccessTypesCache
                    .Find(argsType)
                    .ArgCode()
                    .Call(instance.FunctionId, result.Size);

            return result;
        }

        internal Result GetterResult(Category category, TypeBase argsType) { return CallResult(category, true, argsType); }
        internal Result SetterResult(Category category, TypeBase argsType) { return CallResult(category, false, argsType); }
    }
}