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
        internal TTarget SaveTarget
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

        internal Result<TTarget> With(params Issue[] issues)
            => new Result<TTarget>(Target, Issues.plus(issues));

        internal Result<TOutTarget> Convert<TOutTarget>(Func<TTarget, Result<TOutTarget>> converter)
            where TOutTarget : class
        {
            var inner = converter(Target);
            return new Result<TOutTarget>(inner.Target, Issues.plus(inner.Issues));
        }
        internal Result<TOutTarget> Convert<TOutTarget>(Func<TTarget, TOutTarget> converter)
            where TOutTarget : class
        {
            var inner = converter(Target);
            return new Result<TOutTarget>(inner, Issues);
        }
    }
}