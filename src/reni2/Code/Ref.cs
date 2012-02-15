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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Reference to something
    /// </summary>
    [Serializable]
    abstract class Ref : FiberHead
    {
        readonly RefAlignParam _refAlignParam;

        [Node]
        [DisableDump]
        internal readonly Size Offset;

        protected Ref(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        protected override sealed Size GetSize() { return _refAlignParam.RefSize; }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset; } }

        [Node]
        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}