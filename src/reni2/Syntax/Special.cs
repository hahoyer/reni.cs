using HWClassLibrary.Debug;
using Reni.Parser;

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

        [DumpData(true), DumpExcept(null)]
        private readonly Feature _feature;

        public Special(Base left, Token token, Base right, Feature feature)
        {
            _left = left;
            _feature = feature;
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
        internal override Result VirtVisit(Context.Base context, Category category)
        {
            return _feature.Result(context, category, _left, _right);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            var result = "";
            if(_left != null)
                return base.DumpShort();
            result += _token.Name;
            if(_right != null)
                result += "(" + _right.DumpShort() + ")";
            return result;
        }
    }

    internal abstract class Feature {
        abstract public Result Result(Context.Base context, Category category, Base left, Base right);
    }
}