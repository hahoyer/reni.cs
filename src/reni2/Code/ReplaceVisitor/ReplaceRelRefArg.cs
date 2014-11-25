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
using hw.Debug;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    sealed class ReplaceRelRefArg : ReplaceArg
    {
        [EnableDump]
        readonly Size _offset;

        ReplaceRelRefArg(Result actualArg, Size offset)
            : base(actualArg)
        {
            _offset = offset;
            StopByObjectId(9);
        }

        internal ReplaceRelRefArg(Result actualArg)
            : this(actualArg, Size.Create(0)) { }

        [DisableDump]
        protected override CodeBase Actual
        {
            get
            {
                if(_offset.IsZero)
                    return ActualArg.Code;
                return ActualArg.Code.ReferencePlus(Offset);
            }
        }

        [DisableDump]
        Size Offset { get { return _offset; } }

        protected override Visitor<CodeBase> After(Size size) { return new ReplaceRelRefArg(ActualArg, Offset + size); }
    }
}