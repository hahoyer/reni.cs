using System;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class CleanupSyntax : Syntax
    {
        internal readonly ValueSyntax Value;

        public CleanupSyntax(BinaryTree anchor, ValueSyntax value)
            : base(anchor)
        {
            Value = value??new EmptyList(null);
            AssertValid();
        }

        void AssertValid()
        {
            Tracer.Assert(Anchor != null);
            Tracer.Assert(Value != null);
        }

        internal override int LeftDirectChildCount => 0;
        protected override int DirectChildCount => 1;
        protected override Syntax GetDirectChild(int index) => Value;

    }
}