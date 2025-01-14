namespace Reni.Code;

sealed class Holder : DumpableObject
{
    public Holder(int index, int id)
    {
        Index = index;
        Id = id;
    }

    int Id { get; }
    int Index { get; }

    public string Name => "h" + Id + "_" + Index;
}