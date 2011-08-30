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
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Basics
{
    [Dump("Dump")]
    [Serializable]
    internal sealed class Category : ReniObject, IEquatable<Category>
    {
        private readonly bool _code;
        private readonly bool _type;
        private readonly bool _args;
        private readonly bool _size;
        private readonly bool _isDataLess;

        internal Category() { }

        internal Category(bool isDataLess, bool size, bool type, bool code, bool args)
        {
            _code = code;
            _type = type;
            _args = args;
            _isDataLess = isDataLess;
            _size = size;
        }

        [DebuggerHidden]
        public static Category Size { get { return new Category(false, true, false, false, false); } }

        [DebuggerHidden]
        public static Category Type { get { return new Category(false, false, true, false, false); } }

        [DebuggerHidden]
        public static Category Code { get { return new Category(false, false, false, true, false); } }

        [DebuggerHidden]
        public static Category CodeArgs { get { return new Category(false, false, false, false, true); } }

        [DebuggerHidden]
        public static Category IsDataLess { get { return new Category(true, false, false, false, false); } }

        [DebuggerHidden]
        public static Category None { get { return new Category(false, false, false, false, false); } }

        [DebuggerHidden]
        public static Category All { get { return new Category(true, true, true, true, true); } }

        public bool IsNone { get { return !HasAny; } }
        public bool HasCode { get { return _code; } }
        public bool HasType { get { return _type; } }
        public bool HasArgs { get { return _args; } }
        public bool HasSize { get { return _size; } }
        public bool HasIsDataLess { get { return _isDataLess; } }
        public bool HasAny { get { return _code || _type || _args || _size || _isDataLess; } }

        [DebuggerHidden]
        [DisableDump]
        public Category Typed { get { return this | Type; } }
        [DebuggerHidden]
        [DisableDump]
        public Category Argsed { get { return this | CodeArgs; } }
        public Category Replenished
        {
            get
            {
                var result = this;
                if(HasType || HasCode)
                    result |= Size;
                if(HasCode)
                    result |= CodeArgs;
                if(result.HasSize)
                    result |= IsDataLess;
                return result;
            }
        }

        [DebuggerHidden]
        public static Category operator |(Category x, Category y)
        {
            return new Category(x.HasIsDataLess || y.HasIsDataLess, x.HasSize || y.HasSize, x.HasType || y.HasType, x.HasCode || y.HasCode, x.HasArgs || y.HasArgs);
        }

        [DebuggerHidden]
        public static Category operator &(Category x, Category y)
        {
            return new Category(x.HasIsDataLess && y.HasIsDataLess, x.HasSize && y.HasSize, x.HasType && y.HasType, x.HasCode && y.HasCode, x.HasArgs && y.HasArgs);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = _code.GetHashCode();
                result = (result*397) ^ _type.GetHashCode();
                result = (result*397) ^ _args.GetHashCode();
                result = (result*397) ^ _size.GetHashCode();
                result = (result*397) ^ _isDataLess.GetHashCode();
                return result;
            }
        }

        public bool IsEqual(Category x)
        {
            return
                HasCode == x.HasCode
                && HasArgs == x.HasArgs
                && HasSize == x.HasSize
                && HasType == x.HasType
                && HasIsDataLess == x.HasIsDataLess
                ;
        }

        private bool IsLessThan(Category x)
        {
            return
                (!HasCode && x.HasCode)
                || (!HasArgs && x.HasArgs)
                || (!HasSize && x.HasSize)
                || (!HasType && x.HasType)
                || (!HasIsDataLess && x.HasIsDataLess)
                ;
        }

        private bool IsLessThanOrEqual(Category x)
        {
            if(HasCode && !x.HasCode)
                return false;
            if(HasArgs && !x.HasArgs)
                return false;
            if(HasSize && !x.HasSize)
                return false;
            if(HasType && !x.HasType)
                return false;
            if(HasIsDataLess && !x.HasIsDataLess)
                return false;
            return true;
        }

        [DebuggerHidden]
        public static Category operator -(Category x, Category y)
        {
            return new Category(x.HasIsDataLess && !y.HasIsDataLess, x.HasSize && !y.HasSize, x.HasType && !y.HasType, x.HasCode && !y.HasCode, x.HasArgs && !y.HasArgs);
        }

        protected override string Dump(bool isRecursion) { return DumpShort(); }

        internal override string DumpShort()
        {
            var result = "";
            if(HasIsDataLess)
                result += ".IsDataLess.";
            if(HasSize)
                result += ".Size.";
            if(HasType)
                result += ".Type.";
            if(HasArgs)
                result += ".CodeArgs.";
            if(HasCode)
                result += ".Code.";
            result = result.Replace("..", ",").Replace(".", "");
            if(result == "")
                return "none";
            return result;
        }

        public bool Equals(Category obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj._code.Equals(_code)
                   && obj._type.Equals(_type)
                   && obj._args.Equals(_args)
                   && obj._size.Equals(_size)
                   && obj._isDataLess.Equals(_isDataLess)
                ;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != typeof(Category))
                return false;
            return Equals((Category) obj);
        }

        public static bool operator <(Category left, Category right)
        {
            //return left.IsLessThan(right);
            return left != right && left <= right;
        }

        public static bool operator <=(Category left, Category right)
        {
            //return left < right || left == right;
            return left.IsLessThanOrEqual(right);
        }

        public static bool operator >=(Category left, Category right) { return right <= left; }

        public static bool operator >(Category left, Category right) { return right < left; }

        public static bool operator ==(Category left, Category right) { return Equals(left, right); }

        public static bool operator !=(Category left, Category right) { return !Equals(left, right); }
    }
}