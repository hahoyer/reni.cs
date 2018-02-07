using System;
using Bnf.Contexts;
using Bnf.Features;

namespace Bnf.Forms
{
    sealed class Integer : Form, IExpression, IConstant
    {
        public readonly long Value;

        public Integer(Syntax parent, long value)
            : base(parent) => Value = value;

        protected override Result GetResult(Context context) => throw new NotImplementedException();
    }
}