using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class OccurenceDictionary<T> : DumpableObject
        where T : class, IParseSpan, ISourcePartProxy
    {
        [EnableDump]
        internal readonly IDictionary<ILiteral, object> Data;

        [EnableDump]
        internal readonly string[] ToDo;

        [EnableDump]
        readonly string Reference;

        public OccurenceDictionary(string reference)
        {
            ToDo = new[] {reference};
            Reference = reference;
        }

        OccurenceDictionary(string[] toDo, IDictionary<ILiteral, object> data)
        {
            ToDo = toDo;
            Data = data;
        }

        public OccurenceDictionary(Literal destination)
        {
            ToDo = new string[0];
            Data = new Dictionary<ILiteral, object> {[destination.Value] = destination};
        }

        public OccurenceDictionary<T> AssignTo(string name)
        {
            if(name == Reference)
                NotImplementedMethod(name);

            if(Data == null)
                return this;

            NotImplementedMethod(name);
            return null;
        }

        KeyValuePair<ILiteral, object>
            KeyValuePair(string name, KeyValuePair<ILiteral, object> pair)
            => new KeyValuePair<ILiteral, object>(pair.Key, AssignTo(name, pair.Value));

        object AssignTo(string name, object value)
        {
            NotImplementedMethod(name, value);
            return null;
        }

        public OccurenceDictionary<T> Repeat()
        {
            NotImplementedMethod();
            return null;
        }
    }
}