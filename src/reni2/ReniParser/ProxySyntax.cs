using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class ProxySyntax : Syntax
    {
        public ProxySyntax(Syntax value, Validation.SyntaxError issue)
        {
            Value = value;
            Issue = issue;
        }

        [EnableDump]
        Syntax Value { get; }
        [EnableDump]
        Validation.SyntaxError Issue { get; }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return Value;
                yield return Issue;
            }
        }

        internal override CompileSyntax ToCompiledSyntax
            => new ProxyCompileSyntax(Value.ToCompiledSyntax, Issue);
    }

    sealed class ProxyCompileSyntax : CompileSyntax
    {
        public ProxyCompileSyntax(CompileSyntax value, Validation.SyntaxError issue)
        {
            Value = value;
            Issue = issue;
        }

        [EnableDump]
        CompileSyntax Value { get; }
        [EnableDump]
        Validation.SyntaxError Issue { get; }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return Value;
                yield return Issue;
            }
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Value.Result(context, category) + Issue.Result(context, category);

    }


}