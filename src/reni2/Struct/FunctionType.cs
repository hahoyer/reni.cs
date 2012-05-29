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
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionType : TypeBase, ISetterTargetType
    {
        readonly int _index;
        [Node]
        readonly Structure _structure;
        [Node]
        internal readonly TypeBase ArgsType;
        internal readonly bool IsImplicit;
        [Node]
        readonly SetterFunctionType _setter;
        [NotNull]
        [Node]
        readonly GetterFunctionType _getter;

        internal FunctionType(int index, FunctionSyntax body, Structure structure, TypeBase argsType)
        {
            IsImplicit = body.IsImplicit;
            _getter = new GetterFunctionType(this, index, body.Getter);
            _setter = body.Setter == null ? null : new SetterFunctionType(this, index, body.Setter);
            _index = index;
            _structure = structure;
            ArgsType = argsType;
            StopByObjectId(-10);
        }

        IReferenceInCode ISetterTargetType.ObjectReference { get { return this; } }
        TypeBase ISetterTargetType.Type { get { return this; } }
        TypeBase ISetterTargetType.ValueType { get { return ValueType; } }

        Size IReferenceInCode.RefSize { get { return RefAlignParam.RefSize; } }
        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        
        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }

        internal TypeBase ValueType { get { return _getter.ReturnType; } }

        [Node]
        [DisableDump]
        internal CodeArgs CodeArgs
        {
            get
            {
                var result = _getter.CodeArgs;
                if(_setter != null)
                    result += _setter.CodeArgs;
                return result;
            }
        }

        [DisableDump]
        public Tuple<CodeBase, CodeBase> BodyCode
        {
            get
            {
                return
                    new Tuple<CodeBase, CodeBase>(_getter.BodyCode, _setter == null ? null : _setter.BodyCode);
            }
        }

        [DisableDump]
        internal override bool IsDataLess { get { return CodeArgs.IsNone && ArgsType.IsDataLess; } }
        [DisableDump]
        internal ContextBase GetterContext { get { return _structure.UniqueContext.UniqueFunctionContext(ArgsType); } }
        [DisableDump]
        internal ContextBase SetterContext { get { return _structure.UniqueContext.UniqueFunctionContext(ArgsType, ValueType); } }

        Result ISetterTargetType.Result(Category category) { return _setter.CallResult(category); }
        
        Result ISetterTargetType.DestinationResult(Category category) { return Result(category, this); }

        protected override Size GetSize() { return CodeArgs.Size + ArgsType.Size; }

        internal void EnsureBodyCode()
        {
            _getter.EnsureBodyCode();
            if(_setter != null)
                _setter.EnsureBodyCode();
        }

        internal FunctionContainer Serialize()
        {
            var getter = _getter.Serialize();
            var setter = _setter == null ? null : _setter.Serialize();
            return new FunctionContainer(getter, setter);
        }


        public FunctionInstanceType Instance(bool isGetter)
        {
            if(isGetter)
                return _getter;
            return _setter;
        }

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

        public Result ApplyResult(Category category)
        {
            return Result
                (category
                 , () => CodeArgs.ToCode().Sequence(ArgsType.ArgCode())
                 , () => CodeArgs + CodeArgs.Arg()
                );
        }

        public Result AssignmentFeatureResult(Category category)
        {
            var result = new Result
                (category
                 , getType: () => _setter
                 , getCode: ArgCode
                 , getArgs: CodeArgs.Arg
                );
            return result;
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, ()=>ValueType);
            base.Search(searchVisitor);
        }
    }
}