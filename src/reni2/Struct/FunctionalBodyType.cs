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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Type;

namespace Reni.Struct
{
    abstract class FunctionalBodyType : TypeBase
    {
        readonly FunctionalBody _parent;

        protected FunctionalBodyType(FunctionalBody parent) { _parent = parent; }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return _parent.Structure; } }
        [DisableDump]
        protected abstract TypeBase ArgsType { get; }
        [DisableDump]
        internal override bool IsDataLess { get { return !_parent.IsObjectForCallRequired; } }
        [DisableDump]
        protected abstract string Tag { get; }
        internal override string DumpPrintText { get { return _parent.Body.DumpPrintText + Tag; } }
        internal override IFunctionalFeature FunctionalFeature { get { return _parent; } }

        protected override Size GetSize() { return _parent.RefAlignParam.RefSize; }
        protected Result ObtainApplyResult(Category category) { return _parent.ObtainApplyResult(category, ArgsType); }
    }
}