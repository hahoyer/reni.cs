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
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Basics
{
    [Dump("Dump")]
    [Serializable]
    sealed class Category : ReniObject, IEquatable<Category>
    {
        readonly bool _code;
        readonly bool _type;
        readonly bool _args;
        readonly bool _size;
        readonly bool _isDataLess;

        static readonly Category[] _cache = new Category[32];

        Category(bool isDataLess, bool size, bool type, bool code, bool args)
        {
            _code = code;
            _type = type;
            _args = args;
            _isDataLess = isDataLess;
            _size = size;
        }

        [DebuggerHidden]
        internal static Category CreateCategory(bool isDataLess = false, bool size = false, bool type = false, bool code = false, bool args = false)
        {
            var result = _cache[IndexFromBool(isDataLess, size, type, code, args)];
            if(result != null)
                return result;
            return _cache[IndexFromBool(isDataLess, size, type, code, args)] = new Category(isDataLess, size, type, code, args);
        }

        static int IndexFromBool(params bool[] data) { return data.Aggregate(0, (c, n) => c * 2 + (n ? 1 : 0)); }

        [DebuggerHidden]
        public static Category Size { get { return CreateCategory(size: true); } }

        [DebuggerHidden]
        public static Category Type { get { return CreateCategory(type: true); } }

        [DebuggerHidden]
        public static Category Code { get { return CreateCategory(code: true); } }

        [DebuggerHidden]
        public static Category CodeArgs { get { return CreateCategory(args: true); } }

        [DebuggerHidden]
        public static Category IsDataLess { get { return CreateCategory(isDataLess: true); } }

        [DebuggerHidden]
        public static Category None { get { return CreateCategory(); } }

        [DebuggerHidden]
        public static Category All { get { return CreateCategory(isDataLess: true, size: true, type: true, code: true, args: true); } }

        public bool IsNone { get { return !HasAny; } }
        public bool HasCode { get { return _code; } }
        public bool HasType { get { return _type; } }
        public bool HasArgs { get { return _args; } }
        public bool HasSize { get { return _size; } }
        public bool HasIsDataLess { get { return _isDataLess; } }
        public bool HasAny { get { return _code || _type || _args || _size || _isDataLess; } }

        [DebuggerHidden]
        [DisableDump]
        public Category IaDataLessed { get { return this | IsDataLess; } }
        [DebuggerHidden]
        [DisableDump]
        public Category Sized { get { return this | Size; } }
        [DebuggerHidden]
        [DisableDump]
        public Category Typed { get { return this | Type; } }
        [DebuggerHidden]
        [DisableDump]
        public Category Coded { get { return this | Code; } }
        [DebuggerHidden]
        [DisableDump]
        public Category CodeArgsed { get { return this | CodeArgs; } }
        public Category Replenished
        {
            get
            {
                var result = this;
                if(result.HasCode)
                {
                    result |= Size;
                    result |= CodeArgs;
                }

                if(result.HasSize)
                    result |= IsDataLess;
                return result;
            }
        }

        internal Category ReplaceArged
        {
            get
            {
                var result = None;
                if(HasCode)
                    result |= Type | Code;
                if(HasArgs)
                    result |= CodeArgs;
                return result;
            }
        }

        internal Category FunctionCall
        {
            get
            {
                var result = this - CodeArgs - Code;
                return HasCode ? result.Sized : result;
            }
        }

        [DebuggerHidden]
        public static Category operator |(Category x, Category y) { return CreateCategory(x.HasIsDataLess || y.HasIsDataLess, x.HasSize || y.HasSize, x.HasType || y.HasType, x.HasCode || y.HasCode, x.HasArgs || y.HasArgs); }

        [DebuggerHidden]
        public static Category operator &(Category x, Category y) { return CreateCategory(x.HasIsDataLess && y.HasIsDataLess, x.HasSize && y.HasSize, x.HasType && y.HasType, x.HasCode && y.HasCode, x.HasArgs && y.HasArgs); }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = _code.GetHashCode();
                result = (result * 397) ^ _type.GetHashCode();
                result = (result * 397) ^ _args.GetHashCode();
                result = (result * 397) ^ _size.GetHashCode();
                result = (result * 397) ^ _isDataLess.GetHashCode();
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

        bool IsLessThan(Category x)
        {
            return
                (!HasCode && x.HasCode)
                || (!HasArgs && x.HasArgs)
                || (!HasSize && x.HasSize)
                || (!HasType && x.HasType)
                || (!HasIsDataLess && x.HasIsDataLess)
                ;
        }

        bool IsLessThanOrEqual(Category x)
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
        public static Category operator -(Category x, Category y) { return CreateCategory(x.HasIsDataLess && !y.HasIsDataLess, x.HasSize && !y.HasSize, x.HasType && !y.HasType, x.HasCode && !y.HasCode, x.HasArgs && !y.HasArgs); }

        protected override string Dump(bool isRecursion) { return GetNodeDump(); }

        internal override string GetNodeDump()
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