using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    interface ISeparatorType
    {
        string Text { get; }
        ISeparatorType Escalate(Func<ISeparatorType> func);
    }

    static class SeparatorType
    {
        internal static readonly ISeparatorType None = new NoneType();
        internal static readonly ISeparatorType Contact = new ConcatType("");
        internal static readonly ISeparatorType Close = new ConcatType(" ");
        internal static readonly ISeparatorType Multiline = new ConcatType("\n");

        sealed class NoneType : DumpableObject, ISeparatorType
        {
            string ISeparatorType.Text => null;

            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func)
            {
                NotImplementedMethod(func());
                return null;
            }
        }

        sealed class ConcatType : DumpableObject, ISeparatorType
        {
            internal readonly string Separator;
            internal ConcatType(string separator) { Separator = separator; }

            string ISeparatorType.Text => Separator;

            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func)
            {
                if(this == Multiline)
                    return this;
                var other = func();
                if(other == this)
                    return this;
                if(other == Contact)
                    return this;

                if(other == Multiline)
                    return other;
                if(this == Contact)
                    return other;

                NotImplementedMethod(func());
                return null;
            }
        }

        sealed class ClusteredMultiLineType : DumpableObject, ISeparatorType
        {
            readonly string _separator = "\n";

            string ISeparatorType.Text => _separator;

            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func)
            {
                NotImplementedMethod(func());
                return null;
            }
        }
    }
}