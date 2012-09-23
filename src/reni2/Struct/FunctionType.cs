#region Copyright (C) 2012

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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniParser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionType : SetterTargetType
    {
        readonly int _index;
        [Node]
        readonly Structure _structure;
        [Node]
        internal readonly TypeBase ArgsType;
        [Node]
        readonly SetterFunction _setter;
        [NotNull]
        [Node]
        readonly GetterFunction _getter;
        readonly bool _isValid;
        readonly SimpleCache<CodeArgs> _codeArgsCache;
        readonly ResultCache _applyResultCache;

        internal FunctionType(int index, FunctionSyntax body, Structure structure, TypeBase argsType)
        {
            _getter = new GetterFunction(this, index, body.Getter);
            _setter = body.Setter == null ? null : new SetterFunction(this, index, body.Setter);
            _index = index;
            _structure = structure;
            ArgsType = argsType;
            _codeArgsCache = new SimpleCache<CodeArgs>(ObtainCodeArgs);
            _applyResultCache = new ResultCache(ObtainApplyResult, ObtainPendingApplyResult);
            StopByObjectId(-10);
            _isValid = true;
        }

        CodeArgs ObtainCodeArgs()
        {
            var result = _getter.CodeArgs;
            if(_setter != null)
                result += _setter.CodeArgs;
            return result;
        }

        [DisableDump]
        internal override TypeBase ValueType { get { return _getter.ReturnType; } }
        [DisableDump]
        internal override bool IsDataLess
        {
            get
            {
                //Tracer.Assert(_isValid);
                //Tracer.Assert(!CodeArgs.Size.IsZero);
                return CodeArgs.IsNone && ArgsType.IsDataLess;
            }
        }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return _structure; } }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return ValueType; } }

        [Node]
        [DisableDump]
        CodeArgs CodeArgs { get { return _codeArgsCache.Value; } }

        [DisableDump]
        internal FunctionContainer Container
        {
            get
            {
                var getter = _getter.Container;
                var setter = _setter == null ? null : _setter.Container;
                return new FunctionContainer(getter, setter);
            }
        }

        internal override Result SetterResult(Category category) { return _setter.CallResult(category); }
        internal override Result GetterResult(Category category) { return _getter.CallResult(category); }
        internal override Result DestinationResult(Category category) { return Result(category, this); }
        protected override Size GetSize()
        {
            Tracer.Assert(_isValid);
            Tracer.Assert(!CodeArgs.Size.IsZero);
            return ArgsType.Size + CodeArgs.Size;
        }

        internal ContextBase CreateSubContext(bool useValue) { return new Reni.Context.Function(_structure.UniqueContext, ArgsType, useValue ? ValueType : null); }

        public string DumpFunction()
        {
            var result = "\n";
            result += "index=" + _index;
            result += "\n";
            result += "argsType=" + ArgsType.Dump();
            result += "\n";
            result += "context=" + _structure.Dump();
            result += "\n";
            result += "Getter=" + _getter.DumpFunction();
            result += "\n";
            if(_setter != null)
            {
                result += "Setter=" + _setter.DumpFunction();
                result += "\n";
            }
            result += "type=" + ValueType.Dump();
            result += "\n";
            return result;
        }

        Result ObtainApplyResult(Category category)
        {
            if(IsDataLess)
                return Result(category);
            return Result
                (category
                 , () => CodeArgs.ToCode() + ArgsType.ArgCode
                 , () => CodeArgs + CodeArgs.Arg()
                );
        }

        Result ObtainPendingApplyResult(Category category) { return new Result(category, getType:()=>this, getArgs:CodeArgs.Void); }

        public Result ApplyResult(Category category) { return _applyResultCache & category; }

        internal override bool HasQuickSize { get { return false; } }
        internal override void Search(SearchVisitor searchVisitor, ExpressionSyntax syntax)
        {
            searchVisitor.Search(this, () => ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor, syntax);
        }
    }
}