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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    ///     Instance of a function to compile
    /// </summary>
    sealed class Function : ReniObject
    {
        readonly int _index;
        readonly Structure _structure;
        readonly TypeBase _argsType;

        readonly SetterFunctionInstance _setter;
        [NotNull]
        readonly GetterFunctionInstance _getter;

        internal Function(int index, FunctionSyntax body, Structure structure, TypeBase argsType)
            : base(index)
        {
            _getter = new GetterFunctionInstance(index, body.Getter, argsType, GetGetterContext);
            _setter = body.Setter == null ? null : new SetterFunctionInstance(index, body.Setter, argsType, GetSetterContext);
            _index = index;
            _structure = structure;
            _argsType = argsType;
            StopByObjectId(-10);
        }

        ContextBase GetGetterContext() { return _structure.UniqueContext.UniqueFunctionContext(_argsType); }
        ContextBase GetSetterContext() { return _structure.UniqueContext.UniqueFunctionContext(_argsType, ValueType); }

        [Node]
        [DisableDump]
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

        internal void EnsureBodyCode()
        {
            _getter.EnsureBodyCode();
            if(_setter != null)
                _setter.EnsureBodyCode();
        }

        public Tuple<CodeBase, CodeBase> BodyCode
        {
            get
            {
                return
                    new Tuple<CodeBase, CodeBase>(_getter.BodyCode, _setter == null ? null : _setter.BodyCode);
            }
        }

        internal FunctionContainer Serialize()
        {
            var getter = _getter.Serialize();
            var setter = _setter == null ? null : _setter.Serialize();
            return new FunctionContainer(getter, setter);
        }


        public FunctionInstance Instance(bool isGetter)
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
            result += "argsType=" + _argsType.Dump();
            result += "\n";
            result += "context=" + _structure.Dump();
            result += "\n";
            result += "Getter=" + _getter.DumpFunction();
            result += "\n";
            if (_setter != null)
            {
                result += "Setter=" + _setter.DumpFunction();
                result += "\n";
            }
            result += "type=" + ValueType.Dump();
            result += "\n";
            return result;
        }
    }
}