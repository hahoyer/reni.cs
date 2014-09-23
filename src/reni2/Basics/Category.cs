using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;

namespace Reni.Basics
{
    [Dump("Dump")]
    sealed class Category : DumpableObject, IEquatable<Category>
    {
        readonly bool _code;
        readonly bool _type;
        readonly bool _exts;
        readonly bool _size;
        readonly bool _hllw;

        static readonly Category[] _cache = new Category[32];

        Category(bool hllw, bool size, bool type, bool code, bool exts)
            : base(null)
        {
            _code = code;
            _type = type;
            _exts = exts;
            _hllw = hllw;
            _size = size;
        }

        [DebuggerHidden]
        internal static Category CreateCategory
            (bool hllw = false, bool size = false, bool type = false, bool code = false, bool exts = false)
        {
            var result = _cache[IndexFromBool(hllw, size, type, code, exts)];
            if(result != null)
                return result;
            return _cache[IndexFromBool(hllw, size, type, code, exts)] = new Category(hllw, size, type, code, exts);
        }

        static int IndexFromBool(params bool[] data) { return data.Aggregate(0, (c, n) => c * 2 + (n ? 1 : 0)); }

        [DebuggerHidden]
        public static Category Size { get { return CreateCategory(size: true); } }

        [DebuggerHidden]
        public static Category Type { get { return CreateCategory(type: true); } }

        [DebuggerHidden]
        public static Category Code { get { return CreateCategory(code: true); } }

        [DebuggerHidden]
        public static Category Exts { get { return CreateCategory(exts: true); } }

        [DebuggerHidden]
        public static Category Hllw { get { return CreateCategory(true); } }

        [DebuggerHidden]
        public static Category None { get { return CreateCategory(); } }

        [DebuggerHidden]
        public static Category All { get { return CreateCategory(true, true, true, true, true); } }

        public bool IsNone { get { return !HasAny; } }
        public bool HasCode { get { return _code; } }
        public bool HasType { get { return _type; } }
        public bool HasExts { get { return _exts; } }
        public bool HasSize { get { return _size; } }
        public bool HasHllw { get { return _hllw; } }
        public bool HasAny { get { return _code || _type || _exts || _size || _hllw; } }

        [DebuggerHidden]
        [DisableDump]
        public Category Hllwed { get { return this | Hllw; } }

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
        public Category CodeExtsed { get { return this | Exts; } }

        public Category Replenished
        {
            get
            {
                var result = this;
                if(result.HasCode)
                {
                    result |= Size;
                    result |= Exts;
                }

                if(result.HasSize)
                    result |= Hllw;
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
                if(HasExts)
                    result |= Exts;
                return result;
            }
        }

        internal Category FunctionCall
        {
            get
            {
                var result = this - Exts - Code;
                return HasCode ? result.Sized : result;
            }
        }

        [DebuggerHidden]
        public static Category operator |(Category x, Category y)
        {
            return CreateCategory
                (
                    x.HasHllw || y.HasHllw,
                    x.HasSize || y.HasSize,
                    x.HasType || y.HasType,
                    x.HasCode || y.HasCode,
                    x.HasExts || y.HasExts);
        }

        [DebuggerHidden]
        public static Category operator &(Category x, Category y)
        {
            return CreateCategory
                (
                    x.HasHllw && y.HasHllw,
                    x.HasSize && y.HasSize,
                    x.HasType && y.HasType,
                    x.HasCode && y.HasCode,
                    x.HasExts && y.HasExts);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = _code.GetHashCode();
                result = (result * 397) ^ _type.GetHashCode();
                result = (result * 397) ^ _exts.GetHashCode();
                result = (result * 397) ^ _size.GetHashCode();
                result = (result * 397) ^ _hllw.GetHashCode();
                return result;
            }
        }

        public bool IsEqual(Category x)
        {
            return
                HasCode == x.HasCode
                    && HasExts == x.HasExts
                    && HasSize == x.HasSize
                    && HasType == x.HasType
                    && HasHllw == x.HasHllw
                ;
        }

        bool IsLessThan(Category x)
        {
            return
                (!HasCode && x.HasCode)
                    || (!HasExts && x.HasExts)
                    || (!HasSize && x.HasSize)
                    || (!HasType && x.HasType)
                    || (!HasHllw && x.HasHllw)
                ;
        }

        bool IsLessThanOrEqual(Category x)
        {
            if(HasCode && !x.HasCode)
                return false;
            if(HasExts && !x.HasExts)
                return false;
            if(HasSize && !x.HasSize)
                return false;
            if(HasType && !x.HasType)
                return false;
            if(HasHllw && !x.HasHllw)
                return false;
            return true;
        }

        [DebuggerHidden]
        public static Category operator -(Category x, Category y)
        {
            return CreateCategory
                (
                    x.HasHllw && !y.HasHllw,
                    x.HasSize && !y.HasSize,
                    x.HasType && !y.HasType,
                    x.HasCode && !y.HasCode,
                    x.HasExts && !y.HasExts);
        }

        protected override string Dump(bool isRecursion) { return NodeDump; }

        protected override string GetNodeDump()
        {
            var result = "";
            if(HasHllw)
                result += ".Hllw.";
            if(HasSize)
                result += ".Size.";
            if(HasType)
                result += ".Type.";
            if(HasExts)
                result += ".Exts.";
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
                && obj._exts.Equals(_exts)
                && obj._size.Equals(_size)
                && obj._hllw.Equals(_hllw)
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