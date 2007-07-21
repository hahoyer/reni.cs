using HWClassLibrary.Debug;

namespace Reni.Type
{
    /// <summary>
    /// Fixed sized array of a type
    /// </summary>
    public class Array: Child
    {
        int _count;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="element">Type of elements</param>
        /// <param name="count">DigitChain of elements</param>
        public Array(Base element, int count) : base(element)
        {
            _count = count;
            Tracer.Assert(count > 0);
        }

        /// <summary>
        /// asis
        /// </summary>
        public int Count { get { return _count; } }

        /// <summary>
        /// asis
        /// </summary>
        public override Size Size { get { return Element.Size * _count; } }


        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        public override string DumpPrintText { get { return "("+Element.DumpPrintText+")array("+Count+")"; } }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        public override Result DestructorHandler(Category category)
        {
            return Element.ArrayDestructorHandler(category,Count);
        }

        /// <summary>
        /// asis
        /// </summary>
        public Base Element { get { return Parent; } }
        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        public override Result MoveHandler(Category category)
        {
            return Element.ArrayMoveHandler(category, Count);
        }

        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal override Result DumpPrint(Category category)
        {
            return Element.ArrayDumpPrint(category, Count);
        }

        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 21:18 on HAHOYER-DELL by hh
        public override string Dump()
        {
            return GetType().FullName + "("+Element.Dump()+", "+Count+")";
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        public override Result ConvertToVirt(Category category, Base dest)
        {
            Array destArray = dest as Array;
            if (destArray != null)
            {
                Result result = Element.ConvertTo(category, destArray.Element);
                NotImplementedMethod(category, dest, "result", result);
                return null;
            }
            NotImplementedMethod(category, dest);
            return null;
        }

        /// <summary>
        /// Determines whether [is convertable to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="useConverter">if set to <c>true</c> [use converter].</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 11.01.2007 22:09
        public override bool IsConvertableToVirt(Base dest, bool useConverter)
        {
            Array destArray = dest as Array;
            if (destArray != null)
            {
                if (Count == destArray.Count)
                    return Element.IsConvertableTo(destArray.Element, false);
                return false;
            }
            return false;
        }
    }
}
