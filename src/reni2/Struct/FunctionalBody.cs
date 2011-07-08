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
        private readonly ICompileSyntax _body;
        private readonly Structure _structure;
        private readonly SimpleCache<Type> _typeCache;

        internal FunctionalBody(Structure structure, ICompileSyntax body)
        {
            _structure = structure;
            _body = body;
            _typeCache = new SimpleCache<Type>(() => new Type(this));
            StopByObjectId(1);
        }

        private sealed class Type : TypeBase
        {
            private readonly FunctionalBody _parent;
            public Type(FunctionalBody parent) { _parent = parent; }

            [DisableDump]
            internal override Structure FindRecentStructure { get { return _parent._structure; } }

            protected override Size GetSize() { return Size.Zero; }
            internal override string DumpPrintText { get { return _parent._body.DumpShort() + "/\\"; } }
            internal override IFunctionalFeature FunctionalFeature { get { return _parent; } }
        }

        [DisableDump]
        internal ICompileSyntax Body { get { return _body; } }

        [DisableDump]
        protected override TypeBase ObjectType { get { return _structure.Type; } }

        internal override string DumpShort() { return base.DumpShort() + "(" + _body.DumpShort() + ")/\\" + "#(#in context." + _structure.ObjectId + "#)#"; }

        protected override Result Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            var argsResult = argsType.ArgResult(category | Category.Type);
            return _structure
                .CreateFunctionCall(category, Body, argsResult);
        }

        internal Result DumpPrintResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result Result(Category category) { return ToType().Result(category); }
        private Type ToType() { return _typeCache.Value; }
    }
}