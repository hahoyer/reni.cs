using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Checked<T> : DumpableObject
        where T: class
    {
        public Checked(T value, params Issue[] issues)
        {
            Value = value;
            Issues = issues ?? new Issue[0];
            Tracer.Assert(Value != null);
        }

        internal T Value { get; }
        internal Issue[] Issues { get; }

        [DisableDump]
        internal T SaveValue
        {
            get
            {
                Tracer.Assert(!Issues.Any());
                return Value;
            }
        }

        public static implicit operator Checked<T>(T value)
            => new Checked<T>(value);

        public static Checked<T> From<TIn>(Checked<TIn> x)
            where TIn : class, T
            => new Checked<T>(x.Value, x.Issues);

        internal Checked<T> With(IEnumerable<Issue> issues) => new Checked<T>(Value, Issues.plus(issues));
    }
}