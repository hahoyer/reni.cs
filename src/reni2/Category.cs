using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni
{
    /// <summary>
    /// Used for the compiler visitor and the result objects to choose the categories.
    /// Categories are: <see cref="Size"/>Size, <see cref="Type"/>Type, <see cref="Refs"/>References and <see cref="CodeBase"/>Code
    /// </summary>
    [Dump("Dump")]
    [Serializable]
    internal class Category : ReniObject, IEquatable<Category>
    {
        private readonly bool _code;
        private readonly bool _type;
        private readonly bool _refs;
        private readonly bool _size;

        public Category()
        {
        }

        internal Category(bool size, bool type, bool code, bool refs)
        {
            _code = code;
            _type = type;
            _refs = refs;
            _size = size;
        }

        [DebuggerHidden]
        public static Category Size { get { return new Category(true, false, false, false); } }
        [DebuggerHidden]
        public static Category Type { get { return new Category(false, true, false, false); } }
        [DebuggerHidden]
        public static Category Code { get { return new Category(false, false, true, false); } }
        [DebuggerHidden]
        public static Category Refs { get { return new Category(false, false, false, true); } }
        [DebuggerHidden]
        public static Category None { get { return new Category(false, false, false, false); } }

        public bool IsNone { get { return !(_code || _type || _refs || _size); } }
        public bool HasCode { get { return _code; } }
        public bool HasType { get { return _type; } }
        public bool HasRefs { get { return _refs; } }
        public bool HasSize { get { return _size; } }

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static Category operator |(Category x, Category y)
        {
            return new Category(
                x.HasSize || y.HasSize,
                x.HasType || y.HasType,
                x.HasCode || y.HasCode,
                x.HasRefs || y.HasRefs
                );
        }

        [DebuggerHidden]
        public static Category operator &(Category x, Category y)
        {
            return new Category(
                x.HasSize && y.HasSize,
                x.HasType && y.HasType,
                x.HasCode && y.HasCode,
                x.HasRefs && y.HasRefs);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _code.GetHashCode();
                result = (result*397) ^ _type.GetHashCode();
                result = (result*397) ^ _refs.GetHashCode();
                result = (result*397) ^ _size.GetHashCode();
                return result;
            }
        }

        public bool IsEqual(Category x)
        {
            return
                HasCode == x.HasCode
                && HasRefs == x.HasRefs
                && HasSize == x.HasSize
                && HasType == x.HasType
                ;
        }

        private bool IsLessThan(Category x)
        {
            return
                (!HasCode && x.HasCode)
                || (!HasRefs && x.HasRefs)
                || (!HasSize && x.HasSize)
                || (!HasType && x.HasType)
                ;
        }

        private bool IsLessThanOrEqual(Category x)
        {
            if (HasCode && !x.HasCode)
                return false;
            if (HasRefs && !x.HasRefs)
                return false;
            if (HasSize && !x.HasSize)
                return false;
            if (HasType && !x.HasType)
                return false;
            return true;
        }

        [DebuggerHidden]
        public static Category operator -(Category x, Category y)
        {
            return new Category(
                x.HasSize && !y.HasSize,
                x.HasType && !y.HasType,
                x.HasCode && !y.HasCode,
                x.HasRefs && !y.HasRefs);
        }

        /// <summary>
        /// dump 
        /// </summary>
        /// <returns></returns>
        public override string Dump()
        {
            string result = "";
            if(HasSize) result += ".Size.";
            if(HasType) result += ".Type.";
            if(HasRefs) result += ".Refs.";
            if(HasCode) result += ".Code.";
            result = result.Replace("..", ",").Replace(".", "");
            if (result == "")
                return "none";
            return result;

        }

        public bool Equals(Category obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj._code.Equals(_code) && obj._type.Equals(_type) && obj._refs.Equals(_refs) && obj._size.Equals(_size);
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

        public static bool operator >=(Category left, Category right)
        {
            return right <= left;
        }

        public static bool operator >(Category left, Category right)
        {
            return right < left;
        }

        public static bool operator ==(Category left, Category right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Category left, Category right)
        {
            return !Equals(left, right);
        }

    }
}