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
            public readonly IStatements Statements;

            public Clause(IConstant label, IEnumerable<IStatement> statements)
            {
                Label = label;
                Statements = new Statements(null, statements.ToArray());
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

        protected override Result GetResult(Context context)
        {
            var value = Value.GetResult(context);
            var items = Items.Select(c=>c.Statements.GetResult(context)).Aggregate())

            NotImplementedMethod(context);
            return null;
        }
    }
}