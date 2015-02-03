using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;

namespace Reni.Type
{
    sealed class OptionsData : DumpableObject
    {
        public OptionsData(string id) { Id = id; }

        static char IdChar(bool value) => value ? '.' : ' ';

        int CurrentIndex { get; set; }

        internal sealed class Option : DumpableObject
        {
            OptionsData Parent { get; }
            internal Option(OptionsData parent)
            {
                Parent = parent;
                index = parent.CurrentIndex++;
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

        internal string Id { get; private set; }
        public bool IsValid => CurrentIndex == Id.Length;
        bool this[int index] => Id[index] != ' ';
    }
}