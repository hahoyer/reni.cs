using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
    internal sealed class Special : Base
    {
        [DumpData(true), DumpExcept(null)]
        private readonly Base _left;

        [DumpData(true), DumpExcept(null)]
        private readonly Base _right;

        [DumpData(true), DumpExcept(null)]
        private readonly Token _token;

        private readonly Parser.TokenClass.Special _Special;

        public Special(Base left, Token token, Parser.TokenClass.Special special, Base right)
        {
            _left = left;
            _token = token;
            _Special = special;
            _right = right;
        }

        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        internal override Result VirtVisit(Context.Base context, Category category)
        {
            return _Special.Result(context, category, _left, _token, _right);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            if(_left != null)
                return base.DumpShort();
            var result = _token.Name;
            if(_right != null)
                result += "(" + _right.DumpShort() + ")";
            return result;
        }
    }
}