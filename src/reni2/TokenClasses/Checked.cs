using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Checked<T> : DumpableObject
    {
        public Checked(T value, params Issue[] issues)
        {
            Value = value;
            Issues = issues ?? new Issue[0];
            Tracer.Assert(Value != null);
        }

        internal T Value { get; }
        internal Issue[] Issues { get; }

        public static implicit operator Checked<T>(T value)
            => new Checked<T>(value);
    }
}