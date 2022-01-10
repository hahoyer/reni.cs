namespace Reni.TokenClasses.Whitespace;

sealed class SeparatorRequests
{
    internal bool Head;
    internal bool Inner;
    internal bool Tail;
    internal bool Flat;

    internal bool Get(bool isFirst, bool isLast)
        => isFirst
            ? isLast
                ? Flat
                : Head
            : isLast
                ? Tail
                : Inner;
}