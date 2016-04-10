using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class Result<TTarget> : DumpableObject
        where TTarget : class
    {
        public Result(TTarget target, params Issue[] issues)
        {
            Target = target;
            Issues = issues ?? new Issue[0];
            Tracer.Assert(Target != null);
        }

        internal TTarget Target { get; }
        internal Issue[] Issues { get; }

        [DisableDump]
        internal TTarget SaveValue
        {
            get
            {
                Tracer.Assert(!Issues.Any());
                return Target;
            }
        }

        public static implicit operator Result<TTarget>(TTarget value)
            => new Result<TTarget>(value);

        public static Result<TTarget> From<TIn>(Result<TIn> x)
            where TIn : class, TTarget
            => new Result<TTarget>(x.Target, x.Issues);

        internal Result<TTarget> With(IEnumerable<Issue> issues)
            => new Result<TTarget>(Target, Issues.plus(issues));
    }
}