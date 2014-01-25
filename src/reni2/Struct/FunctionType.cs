#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using hw.Debug;
using hw.Forms;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
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

        internal FunctionType(int index, FunctionSyntax body, Structure structure, TypeBase argsType)
        {
            _getter = new GetterFunction(this, index, body.Getter);
            _setter = body.Setter == null ? null : new SetterFunction(this, index, body.Setter);
            _index = index;
            _structure = structure;
            ArgsType = argsType;
            StopByObjectId(-10);
        }

        CodeArgs ObtainCodeArgs()
        {
            var result = _getter.CodeArgs;
            Tracer.Assert(result != null);
            if(_setter != null)
                result += _setter.CodeArgs;
            return result;
        }

        [DisableDump]
        internal override TypeBase ValueType { get { return _getter.ReturnType; } }
        [DisableDump]
        internal override bool IsDataLess { get { return CodeArgs.IsNone && ArgsType.IsDataLess; } }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return _structure; } }

        [Node]
        [DisableDump]
        CodeArgs CodeArgs { get { return ObtainCodeArgs(); } }

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

        protected override Result SetterResult(Category category) { return _setter.CallResult(category); }
        protected override Result GetterResult(Category category) { return _getter.CallResult(category); }
        protected override Size GetSize() { return ArgsType.Size + CodeArgs.Size; }

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

        // remark: watch out when caching anything here. 
        // This may hinder the recursive call detection, located at result cache of context. 
        public Result ApplyResult(Category category)
        {
            var result = Result
                (category
                    , () => CodeArgs.ToCode() + ArgsType.ArgCode
                    , () => CodeArgs + CodeArgs.Arg()
                );
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        internal override bool HasQuickSize { get { return false; } }
    }
}