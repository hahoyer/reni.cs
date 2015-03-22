using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

namespace Reni.ReniParser
{
    sealed class ProxySyntax : Syntax
    {
        public ProxySyntax(Validation.SyntaxError issue, params Syntax[] value)
        {
            Value = value.Where(item => item != null).ToArray();
            Issue = issue;
            StopByObjectIds(71);
        }

        [EnableDump]
        Syntax[] Value { get; }
        [EnableDump]
        Validation.SyntaxError Issue { get; }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                foreach(var syntax in Value)
                    yield return syntax;
                yield return Issue;
            }
        }

        internal override CompileSyntax ToCompiledSyntax
            => new ProxyCompileSyntax(Issue, Value.Select(item => item.ToCompiledSyntax).ToArray());
    }

    sealed class ProxyCompileSyntax : CompileSyntax
    {
        public ProxyCompileSyntax(Validation.SyntaxError issue, params CompileSyntax[] value)
        {
            Value = value.Where(item => item != null).ToArray();
            Issue = issue;
            StopByObjectIds(74);
        }

        [EnableDump]
        CompileSyntax[] Value { get; }
        [EnableDump]
        Validation.SyntaxError Issue { get; }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                foreach(var syntax in Value)
                    yield return syntax;
                yield return Issue;
            }
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Value.Select(item => item.Result(context, category))
                .Aggregate(Issue.Result(context, category), (c, n) => c + n);
    }
}