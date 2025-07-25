using hw.Parser;

namespace Reni.TokenClasses.Brackets;

interface IBracket
{
    int Level { get; }
    Setup Setup { get; }
}

interface IRightBracket : IBracket { }
interface ILeftBracket : IBracket;

sealed class Options
{
    internal bool MeansPublic;
    internal bool MeansPositional;
    internal bool MeansKernelPart;
}

sealed class Setup : DumpableObject
{
    public static readonly Setup[] Instances =
    [
        new(PrioTable.BeginOfText, PrioTable.EndOfText)
        , new("{", "}")
        , new("[", "]")
        , new("(", ")", new()
        {
            MeansPublic = true, MeansPositional = true, MeansKernelPart = true
        })
    ];

    internal readonly string OpeningTokenId;
    internal readonly string ClosingTokenId;
    readonly Options? Options;
    internal bool MeansPublic => Options?.MeansPublic ?? false;
    internal bool MeansPositional => Options?.MeansPositional ?? false;
    internal bool MeansKernelPart => Options?.MeansKernelPart ?? false;

    Setup
    (
        string openingTokenId
        , string closingTokenId
        , Options? options = null
    )
    {
        OpeningTokenId = openingTokenId;
        ClosingTokenId = closingTokenId;
        Options = options;
    }
}
