using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Type;

sealed class Flags : DumpableObject
{
    internal readonly List<string> Names = new();
    internal int CurrentIndex { get; set; }
    internal string Id { get; private set; }

    internal string DumpPrintText
        => Names.Select((name, i) => Value(i)? "[" + name + "]" : "").Stringify("");

    internal bool IsValid => CurrentIndex == Id.Length;

    internal Flags(string id) => Id = id;

    internal static char IdChar(bool value) => value? '.' : ' ';

    internal void Align()
    {
        if(Id != null)
            return;
        Id = ("" + IdChar(false)).Repeat(CurrentIndex);
    }

    internal Flag Register(string name) => new(this, name);
    internal bool Value(int index) => Id[index] == IdChar(true);
}

sealed class Flag : DumpableObject
{
    Flags Parent { get; }

    int Index { get; }
    internal bool Value => Parent.Value(Index);

    internal Flag(Flags parent, string name)
    {
        Parent = parent;
        Index = Parent.CurrentIndex++;
        Parent.Names.Add(name);
    }

    internal string SetTo(bool value)
        => Parent.Id.Substring(0, Index)
            + Flags.IdChar(value)
            + Parent.Id.Substring(Index + 1);
}