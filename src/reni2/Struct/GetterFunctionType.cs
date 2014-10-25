#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.ReniSyntax;
using Reni.Type;

namespace Reni.Struct
{
    sealed class GetterFunction : FunctionInstance
    {
        readonly FunctionId _functionId;
        public GetterFunction(FunctionType parent, int index, CompileSyntax body)
            : base(parent, body) { _functionId = FunctionId.Getter(index); }

        [DisableDump]
        internal TypeBase ReturnType { get { return CallResult(Category.Type).Type; } }
        [DisableDump]
        protected override Size RelevantValueSize { get { return Size.Zero; } }
        protected override FunctionId FunctionId { get { return _functionId; } }
    }
}