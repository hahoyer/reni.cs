using System;
using Bnf.Contexts;
using Bnf.Features;

namespace Bnf.Forms
{
    sealed class Index : Form, IIndex
    {
        readonly IExpression Value;

        public Index(Syntax parent, IExpression value)
            : base(parent) => Value = value;

        IExpression IIndex.Value => Value;

        protected override Result GetResult(Context context) => throw new NotImplementedException();
    }
}