using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Stx.Contexts;
using Stx.Features;

namespace Stx.Forms
{
    sealed class Case : Form, IStatement
    {
        internal sealed class Clause : DumpableObject
        {
            public readonly IConstant Label;
            public readonly IStatement[] Statements;

            public Clause(IConstant label, IEnumerable<IStatement> statements)
            {
                Label = label;
                Statements = statements.ToArray();
            }
        }

        [EnableDump]
        public readonly Clause[] Items;

        [EnableDump]
        public readonly IExpression Value;

        public Case(Syntax parent, IExpression value, Clause[] items)
            : base(parent)
        {
            Items = items;
            Value = value;
        }

        protected override Result GetResult(Context context) => throw new NotImplementedException();
    }
}