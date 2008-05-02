using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    internal sealed class Special : SyntaxBase
    {
        [DumpData(true), DumpExcept(null)]
        private readonly SyntaxBase _left;

        [DumpData(true), DumpExcept(null)]
        private readonly SyntaxBase _right;

        private readonly Parser.TokenClass.Special _Special;
        [DumpData(true), DumpExcept(null)]
        private readonly Token _token;

        public Special(SyntaxBase left, Token token, Parser.TokenClass.Special special, SyntaxBase right)
        {
            _left = left;
            _token = token;
            _Special = special;
            _right = right;
        }

        internal protected override string FilePosition
        {
            get
            {
                if(_left != null)
                    return _left.FilePosition;
                return _token.FilePosition;
            }
        }

        internal override Result VirtVisit(ContextBase context, Category category)
        {
            return _Special.Result(context, category, _left, _token, _right);
        }

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