using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace;

sealed class LinesAndSpacesConfiguration : DumpableObject, LinesAndSpaces.IConfiguration
{
    [EnableDump]
    readonly LinesAndSpaces.IConfiguration Configuration;

    [EnableDump]
    readonly LinesAndSpaces.IPredecessor Predecessor;

    [EnableDump]
    readonly bool IsForTail;

    internal LinesAndSpacesConfiguration
        (LinesAndSpaces.IConfiguration configuration, LinesAndSpaces.IPredecessor predecessor)
    {
        Configuration = configuration;
        Predecessor = predecessor;
        IsForTail = true;
        StopByObjectIds();
    }

    int? LinesAndSpaces.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;

    int LinesAndSpaces.IConfiguration.MinimalLineBreakCount => Configuration.MinimalLineBreakCount;

    SeparatorRequests LinesAndSpaces.IConfiguration.SeparatorRequests => Configuration.SeparatorRequests;
}