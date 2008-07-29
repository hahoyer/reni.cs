using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni
{
    /// <summary>
    /// Used for the compiler visitor and the result objects to choose the categories.
    /// Categories are: <see cref="Size"/>Size, <see cref="Type"/>Type, <see cref="Refs"/>References and <see cref="Container"/>Code
    /// </summary>
    [dump("Dump")]
    internal class Category : IEquatable<Category>
    {
        private readonly bool _code;
        private readonly bool _type;
        private readonly bool _refs;
        private readonly bool _size;
        private readonly bool _internal;

        public Category()
        {
        }

        internal Category(bool size, bool type, bool code, bool refs, bool @internal)
        {
            _code = code;
            _internal = @internal;
            _type = type;
            _refs = refs;
            _size = size;
        }

        [DebuggerHidden]
        public static Category Size { get { return new Category(true, false, false, false, false); } }
        [DebuggerHidden]
        public static Category Type { get { return new Category(false, true, false, false, false); } }
        [DebuggerHidden]
        public static Category Code { get { return new Category(false, false, true, false, false); } }
        [DebuggerHidden]
        public static Category Refs { get { return new Category(false, false, false, true, false); } }
        [DebuggerHidden]
        public static Category Internal { get { return new Category(false, false, false, false, true); } }
        [DebuggerHidden]
        public static Category ForInternal { get { return new Category(true, true, true, true, false); } }

        public bool IsNull { get { return !(_code || _type || _refs || _size || _internal); } }
        public bool HasCode { get { return _code; } }
        public bool HasType { get { return _type; } }
        public bool HasRefs { get { return _refs; } }
        public bool HasSize { get { return _size; } }
        public bool HasInternal { get { return _internal; } }
        public bool HasAll { get { return HasCode && HasRefs && HasSize && HasType; } }

        /// <summary>
        /// Some categories are dependent. This function replendishes those categories.
        /// Rules are: type and code results in adding size, code results in adding references
        /// </summary>
        /// <returns></returns>
        public Category Replendish()
        {
            Category Return = this;
            if (HasType || HasCode)
                Return |= Size;
            if (HasCode)
                Return |= Refs;
            return Return;
        }

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
                x.HasRefs || y.HasRefs,
                x.HasInternal || y.HasInternal
                );
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Category operator &(Category x, Category y)
        {
            return new Category(
                x.HasSize && y.HasSize,
                x.HasType && y.HasType,
                x.HasCode && y.HasCode,
                x.HasRefs && y.HasRefs,
                x.HasInternal && y.HasInternal);
        }

        /// <summary>
        /// bit combination
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _code.GetHashCode();
                result = (result*397) ^ _type.GetHashCode();
                result = (result*397) ^ _refs.GetHashCode();
                result = (result*397) ^ _size.GetHashCode();
                result = (result*397) ^ _internal.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Determines whether instance are equal by value
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>
        /// 	<c>true</c> if the specified x is equal; otherwise, <c>false</c>.
        /// </returns>
        /// created 05.01.2007 01:38
        public bool IsEqual(Category x)
        {
            return
                HasCode == x.HasCode
                && HasRefs == x.HasRefs
                && HasSize == x.HasSize
                && HasType == x.HasType
                && HasInternal == x.HasInternal
                ;
        }
        /// <summary>
        /// asis
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Category operator -(Category x, Category y)
        {
            return new Category(
                x.HasSize && !y.HasSize,
                x.HasType && !y.HasType,
                x.HasCode && !y.HasCode,
                x.HasRefs && !y.HasRefs,
                x.HasInternal && !y.HasInternal);
        }

        /// <summary>
        /// dump 
        /// </summary>
        /// <returns></returns>
        public string Dump()
        {
            string result = "";
            if(HasSize) result += ".Size.";
            if(HasType) result += ".Type.";
            if(HasRefs) result += ".Refs.";
            if(HasCode) result += ".Code.";
            if (HasInternal) result += ".Internal.";
            result = result.Replace("..", ",").Replace(".", "");
            if (result == "")
                return "none";
            return result;

        }

        public override string ToString(){return Dump();}

        public bool Equals(Category obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj._code.Equals(_code) && obj._type.Equals(_type) && obj._refs.Equals(_refs) && obj._size.Equals(_size) && obj._internal.Equals(_internal);
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