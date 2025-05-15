using hw.Scanner;

namespace Reni;

sealed class SourceList : DumpableObject, ISourceProvider, IMultiSourceProvider
{
    readonly Source[] Target;

    public SourceList(IEnumerable<ISourceProvider> target)
        => Target = target
            .Select(t => new Source(t, t.Identifier))
            .ToArray();

    SourcePosition IMultiSourceProvider.Position(int position, bool isEnd)
    {
        foreach(var source in Target)
        {
            var end = source.Length;
            if(position < end || (position == end && isEnd))
            {
                if(source.SourceProvider is IMultiSourceProvider provider)
                    return provider.Position(position, isEnd);
                return source + position;
            }

            position -= end;
        }

        throw new ArgumentOutOfRangeException(nameof(position));
    }

    string ISourceProvider.Data => Target.Select(t => t.Data).Aggregate("", (c, n) => c + n);
    string? ISourceProvider.Identifier => null;
    bool ISourceProvider.IsPersistent => Target.Any(t => t.IsPersistent);
    int ISourceProvider.Length => Target.Sum(t => t.Length);
}