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
using System.Diagnostics;
using System.Linq;
using hw.Debug;

namespace Reni.Basics
{
    [Dump("Dump")]
    [DebuggerDisplay("{CodeDump,nq}")]
    sealed class RefAlignParam : IEquatable<RefAlignParam>
    {
        readonly int _alignBits;
        readonly Size _refSize;

        public RefAlignParam(int alignBits, Size refSize)
        {
            _alignBits = alignBits;
            _refSize = refSize;
        }

        public int AlignBits => _alignBits;
        public Size RefSize => _refSize;

        public RefAlignParam Align(int alignBits)
        {
            var newAlignBits = Math.Max(AlignBits, alignBits);
            if(newAlignBits == AlignBits)
                return this;
            return new RefAlignParam(newAlignBits, RefSize);
        }

        public static Size Offset(SizeArray list, int index)
        {
            var result = Size.Create(0);
            for(var i = index + 1; i < list.Count; i++)
                result += list[i];
            return result;
        }

        public bool IsEqual(RefAlignParam param)
        {
            if(param.AlignBits != AlignBits)
                return false;

            if(param.RefSize != RefSize)
                return false;

            return true;
        }

        public string Dump() => "[A:" + AlignBits + ",S:" + RefSize.Dump() + "]";

        internal string CodeDump => AlignBits + "/" + RefSize.ToInt();

        public override int GetHashCode()
        {
            unchecked
            {
                return (_alignBits * 397) ^ (_refSize != null ? _refSize.GetHashCode() : 0);
            }
        }

        public bool Equals(RefAlignParam obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj._alignBits == _alignBits && Equals(obj._refSize, _refSize);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != typeof(RefAlignParam))
                return false;
            return Equals((RefAlignParam) obj);
        }

        public static bool operator ==(RefAlignParam left, RefAlignParam right) => Equals(left, right);

        public static bool operator !=(RefAlignParam left, RefAlignParam right) => !Equals(left, right);
    }
}