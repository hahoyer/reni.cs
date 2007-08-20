using System;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni
{
    /// <summary>
    /// Used for the compiler visitor and the result objects to choose the categories.
    /// Categories are: <see cref="Size"/>Size, <see cref="Type"/>Type, <see cref="Refs"/>References and <see cref="Container"/>Code
    /// </summary>
    [dump("Dump")]
    public class Category
    {
        private readonly bool _code;
        private readonly bool _type;
        private readonly bool _refs;
        private readonly bool _size;

        /// <summary>
        /// constructor, that chosses not category
        /// </summary>
        public Category()
        {
        }

        /// <summary>
        /// constructor, that chooses a combination of categories
        /// </summary>
        /// <param name="size">choose size</param>
        /// <param name="type">choose type</param>
        /// <param name="code">choose code</param>
        /// <param name="refs">choose references</param>
        public Category(bool size, bool type, bool code, bool refs)
        {
            _code = code;
            _type = type;
            _refs = refs;
            _size = size;
        }

        /// <summary>
        /// constructor, that chooses size only
        /// </summary>
        public static Category Size { get { return new Category(true, false, false, false); } }

        /// <summary>
        /// constructor, that chooses type only
        /// </summary>
        public static Category Type { get { return new Category(false, true, false, false); } }

        /// <summary>
        /// constructor, that chooses code only
        /// </summary>
        public static Category Code { get { return new Category(false, false, true, false); } }

        /// <summary>
        /// constructor, that chooses references only
        /// </summary>
        public static Category Refs { get { return new Category(false, false, false, true); } }

        /// <summary>
        /// true if no category is selected
        /// </summary>
        public bool IsNull { get { return !(_code || _type || _refs || _size); } }

        /// <summary>
        /// true if code is selected
        /// </summary>
        public bool HasCode { get { return _code; } }

        /// <summary>
        /// true if type is selected
        /// </summary>
        public bool HasType { get { return _type; } }

        /// <summary>
        /// true if references are selected
        /// </summary>
        public bool HasRefs { get { return _refs; } }

        /// <summary>
        /// true if size is selected
        /// </summary>
        public bool HasSize { get { return _size; } }

        /// <summary>
        /// Gets a value indicating whether this instance has all categories set.
        /// </summary>
        /// <value><c>true</c> if this instance has all categories set; otherwise, <c>false</c>.</value>
        /// created 19.08.2007 22:36 on HAHOYER-DELL by hh
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
        public static Category operator |(Category x, Category y)
        {
            return new Category(
                x.HasSize || y.HasSize,
                x.HasType || y.HasType,
                x.HasCode || y.HasCode,
                x.HasRefs || y.HasRefs);
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
                x.HasRefs && y.HasRefs);
        }

        /// <summary>
        /// bit combination
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (HasSize ? 0 : 1) + 2 * (HasType ? 0 : 1) + 4 * (HasCode ? 0 : 1) + 8 * (HasRefs ? 0 : 1);
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
                x.HasRefs && !y.HasRefs);
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
            result = result.Replace("..",",").Replace(".","");
            if (result == "")
                return "none";
            return result;

        }

        ///<summary>
        ///Returns a <see cref="T:System.Sequence"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Sequence"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString(){return Dump();}

        private string DebuggerDumpString { get { return Dump(); } }

    }
}