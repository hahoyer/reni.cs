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
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionType : TypeBase, ISetterTargetType, ISearchContainerType, IConverter, ISmartReference
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
        [DisableDump]
        internal readonly ISuffixFeature AssignmentFeature;

        internal FunctionType(int index, FunctionSyntax body, Structure structure, TypeBase argsType)
        {
            AssignmentFeature = new AssignmentFeature(this);
            _getter = new GetterFunction(this, index, body.Getter);
            _setter = body.Setter == null ? null : new SetterFunction(this, index, body.Setter);
            _index = index;
            _structure = structure;
            ArgsType = argsType;
            StopByObjectId(-10);
        }

        TypeBase ISetterTargetType.Type { get { return this; } }
        TypeBase ISetterTargetType.ValueType { get { return ValueType; } }
        TypeBase ISmartReference.TargetType { get { return ValueType; } }
        Result ISmartReference.DereferenceResult(Category category) { return _getter.CallResult(category); }
        Size IContextReference.Size { get { return Size; } }
        IConverter ISearchContainerType.Converter { get { return this; } }
        TypeBase ISearchContainerType.Target { get { return ValueType; } }

        internal TypeBase ValueType { get { return _getter.ReturnType; } }
        [DisableDump]
        internal override bool IsDataLess { get { return CodeArgs.IsNone && ArgsType.IsDataLess; } }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return _structure; } }

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
        public CodeBasePair BodyCode
        {
            get
            {
                return
                    new CodeBasePair(_getter.BodyCode, _setter == null ? null : _setter.BodyCode);
            }
        }

        Result ISetterTargetType.Result(Category category) { return _setter.CallResult(category); }
        Result ISetterTargetType.DestinationResult(Category category) { return Result(category, this); }
        Result IConverter.Result(Category category) { return _getter.CallResult(category); }

        protected override Size GetSize() { return ArgsType.Size + CodeArgs.Size; }

        internal void EnsureBodyCode()
        {
            _getter.EnsureBodyCode();
            if(_setter != null)
                _setter.EnsureBodyCode();
        }

        internal ContextBase CreateSubContext(bool useValue) { return new Reni.Context.Function(_structure.UniqueContext, ArgsType, useValue ? ValueType : null); }

        internal FunctionContainer Serialize()
        {
            var getter = _getter.Serialize();
            var setter = _setter == null ? null : _setter.Serialize();
            return new FunctionContainer(getter, setter);
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
            if(IsDataLess)
                return Result(category);
            return Result
                (category
                 , () => CodeArgs.ToCode().Sequence(ArgsType.ArgCode)
                 , () => CodeArgs + CodeArgs.Arg()
                );
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }
    }

    sealed class CodeBasePair : ReniObject
    {
        internal readonly CodeBase Getter;
        internal readonly CodeBase Setter;

        internal CodeBasePair(CodeBase getter, CodeBase setter)
        {
            Getter = getter;
            Setter = setter;
        }
    }
}