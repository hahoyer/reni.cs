using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;

namespace Reni.Formatting
{
    interface ISeparatorType
    {
        string After(string target);
        string Before(string target);
        string Text { get; }
        IEnumerable<string> Grouped(IEnumerable<string> items);
        ISeparatorType Escalate(Func<ISeparatorType> func);
    }

    static class SeparatorType
    {
        internal static readonly ISeparatorType None = new NoneType();
        internal static readonly ISeparatorType Contact = new ConcatType("");
        internal static readonly ISeparatorType Close = new ConcatType(" ");
        internal static readonly ISeparatorType Multiline = new ConcatType("\n");
        internal static readonly ISeparatorType IndentedMultiline = new IndentType();
        internal static readonly ISeparatorType ClusteredMultiLine = new ClusteredMultiLineType();

        sealed class IndentType : DumpableObject, ISeparatorType
        {
            string ISeparatorType.After(string target)
            {
                NotImplementedMethod(target);
                return null;
            }

            string ISeparatorType.Before(string target) => ("\n" + target).Indent();
            string ISeparatorType.Text => "\n";
            IEnumerable<string> ISeparatorType.Grouped(IEnumerable<string> items) => items;
            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func) => this;
        }

        sealed class NoneType : DumpableObject, ISeparatorType
        {
            string ISeparatorType.After(string target)
            {
                Tracer.Assert(target == null);
                return "";
            }

            string ISeparatorType.Before(string target)
            {
                Tracer.Assert(target == null);
                return "";
            }

            string ISeparatorType.Text => null;
            IEnumerable<string> ISeparatorType.Grouped(IEnumerable<string> items) => items;

            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func)
            {
                NotImplementedMethod(func());
                return null;
            }
        }

        sealed class ConcatType : DumpableObject, ISeparatorType
        {
            readonly string _separator;
            internal ConcatType(string separator) { _separator = separator; }

            string ISeparatorType.After(string target) => target + _separator;
            string ISeparatorType.Before(string target) => _separator + target;
            string ISeparatorType.Text => _separator;
            IEnumerable<string> ISeparatorType.Grouped(IEnumerable<string> items) => items;

            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func)
            {
                var result = func();
                return result.Text == "\n" ? IndentedMultiline : result;
            }
        }

        sealed class ClusteredMultiLineType : DumpableObject, ISeparatorType
        {
            readonly string _separator = "\n";

            string ISeparatorType.After(string target) => target + _separator;
            string ISeparatorType.Before(string target) => _separator + target;
            string ISeparatorType.Text => _separator;

            IEnumerable<string> ISeparatorType.Grouped(IEnumerable<string> items)
            {
                var lastWasMultilline = false;
                foreach(var item in items)
                {
                    var isMultiline = item.Contains(_separator);
                    if(lastWasMultilline || isMultiline)
                        yield return _separator + item;
                    else
                        yield return item;
                    lastWasMultilline = isMultiline;
                }
            }

            ISeparatorType ISeparatorType.Escalate(Func<ISeparatorType> func)
            {
                NotImplementedMethod(func());
                return null;
            }
        }
    }
}