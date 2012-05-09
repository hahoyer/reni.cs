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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionalBodyType : TypeBase, IFunctionalFeature
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly CompileSyntax _getter;
        [EnableDump]
        readonly CompileSyntax _setter;
        readonly bool _isImplicit;
        readonly DictionaryEx<TypeBase, FunctionAccessType> _functionAccessTypesCache;

        internal FunctionalBodyType(Structure structure, CompileSyntax getter, CompileSyntax setter, bool isImplicit)
        {
            _structure = structure;
            _getter = getter;
            _setter = setter;
            _isImplicit = isImplicit;
            _functionAccessTypesCache = new DictionaryEx<TypeBase, FunctionAccessType>(argsType => new FunctionAccessType(this, argsType));
        }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return _structure; } }
        [DisableDump]
        internal override bool IsLambda { get { return true; } }
        [DisableDump]
        internal override bool IsDataLess { get { return _structure.IsDataLess || !(_structure.IsObjectForCallRequired(_getter) || _structure.IsObjectForCallRequired(_setter)); } }
        [DisableDump]
        internal override bool IsLikeReference { get { return true; } }

        string Tag { get { return _isImplicit ? "/!\\" : "/\\"; } }
        internal override string DumpPrintText { get { return _getter.DumpPrintText + Tag + _setter.DumpPrintText; } }
        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return this; } }

        protected override Size GetSize() { return _structure.RefAlignParam.RefSize; }

        [DisableDump]
        bool IFunctionalFeature.IsImplicit { get { return _isImplicit; } }

        internal CodeArgs GetCodeArgs(TypeBase argsType)
        {
            var codeArgs = CodeArgs.Void();
            if(_setter != null)
                codeArgs += SetterResult(Category.CodeArgs, argsType, ValueType(argsType)).CodeArgs;
            if(_getter != null)
                codeArgs += GetterResult(Category.CodeArgs, argsType).CodeArgs;
            return codeArgs - CodeArgs.Arg();
        }

        Result IFunctionalFeature.ApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            return _functionAccessTypesCache
                .Find(argsType)
                .ApplyResult(category);
        }

        internal TypeBase ValueType(TypeBase argsType)
        {
            if(_getter == null)
                return null;
            return GetterResult(Category.Type, argsType).Type;
        }

        internal Result SetterResult(Category category, TypeBase argsType, TypeBase valueType)
        {
            Tracer.Assert(!_isImplicit);
            return _structure
                .UniqueContext
                .UniqueFunctionContext(argsType, valueType)
                .UniqueResultWithReplace(category, _setter);
        }

        internal Result GetterResult(Category category, TypeBase argsType)
        {
            Tracer.Assert(!_isImplicit);
            return _structure
                .UniqueContext
                .UniqueFunctionContext(argsType)
                .UniqueResultWithReplace(category, _getter);
        }
    }
}