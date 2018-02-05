using System;
using Stx.Contexts;
using Stx.Features;

namespace Stx.Forms
{
    sealed class Integer : Form, IExpression, IConstant
    {
        public readonly long Value;

        public Integer(Syntax parent, long value)
            : base(parent) => Value = value;

        protected override Result GetResult(Context context) => throw new NotImplementedException();
    }
}