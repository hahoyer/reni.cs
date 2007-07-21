using System;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.Syntax
{
    sealed internal class Special : Base
    {
        [DumpData(true)]
        private readonly Base _left;
        [DumpData(true)]
        private readonly Token _token;
        [DumpData(true)]
        private readonly Base _right;

        public Special(Base left, Token token, Base right)
        {
            _left = left;
            _token = token;
            _right = right;
        }

        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        public override Result VirtVisit(Context.Base context, Category category)
        {
            return _token.Result(context, category, _left, _right);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            string result = "";
            if(_left != null)
                return base.DumpShort();
            result += _token.Name;
            if (_right != null)
                result += "(" + _right.DumpShort() + ")";
            return result;
        }
    }
}

