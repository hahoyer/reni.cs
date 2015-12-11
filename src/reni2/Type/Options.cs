using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.Type
{
    sealed class OptionsData : DumpableObject
    {
        readonly List<string> _names = new List<string>();
        int CurrentIndex { get; set; }

        public OptionsData(string id) { Id = id; }

        static char IdChar(bool value) => value ? '.' : ' ';

        internal sealed class Option : DumpableObject
        {
            OptionsData Parent { get; }
            internal Option(OptionsData parent, string name)
            {
                Parent = parent;
                index = Parent.CurrentIndex++;
                Parent._names.Add(name);
            }
            int index { get; }
            public bool Value => Parent[index];

            public string SetTo(bool value)
                => Parent.Id.Substring(0, index)
                    + IdChar(value)
                    + Parent.Id.Substring(index + 1);
        }

        internal void Align()
        {
            if(Id != null)
                return;
            Id = ("" + IdChar(false)).Repeat(CurrentIndex);
        }

        public string DumpPrintText => _names.Select((name, i) => this[i] ? "[" + name + "]" : "").Stringify("");
        internal string Id { get; private set; }
        public bool IsValid => CurrentIndex == Id.Length;
        bool this[int index] => Id[index] != ' ';
    }
}