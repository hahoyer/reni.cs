using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;

namespace Reni.Type
{
    /// <summary>
    /// Pair of types
    /// </summary>
    internal class Pair : Child
    {
        private readonly Base _second;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// created 19.11.2006 22:57
        internal Pair(Base first, Base second) : base(first)
        {
            _second = second;
        }

        /// <summary>
        /// Gets the first.
        /// </summary>
        /// <value>The first.</value>
        /// created 19.11.2006 22:59
        internal Base First { get { return Parent; } }

        /// <summary>
        /// Gets the second.
        /// </summary>
        /// <value>The second.</value>
        /// created 19.11.2006 22:59
        [Node]
        internal Base Second { get { return _second; } }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return First.Size + Second.Size; } }

        [DumpData(false)]
        internal override string DumpPrintText
        {
            get
            {
                var result = "";
                var types = ToList;
                for(var i = 0; i < types.Length; i++)
                {
                    result += "\n";
                    result += types[i];
                }
                return "(" + HWString.Indent(result) + "\n)";
            }
        }

        [DumpData(false)]
        internal protected override Base[] ToList
        {
            get
            {
                var result = new List<Base>(Parent.ToList) {Second};
                return result.ToArray();
            }
        }

        protected override TypePath ChildTypePath { get { NotImplementedMethod();return null; } }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            var firstHandler = First.DestructorHandler(category);
            var secondHandler = Second.DestructorHandler(category);
            if(firstHandler.IsEmpty)
                return secondHandler;
            if(secondHandler.IsEmpty)
                return firstHandler;

            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            var result = DumpPrintArrayFromRef(category, refAlignParam);
            return Result.ConcatPrintResult(category, result);
        }

        private List<Result> DumpPrintArrayFromRef(Category category, RefAlignParam refAlignParam)
        {
            var result = new List<Result>();
            var list = ToList;
            for(var i = 0; i < list.Length; i++)
            {
                var iResult = list[i].DumpPrintFromRef(category, refAlignParam);
                result.Add(iResult);
            }
            return result;
        }
    }
}