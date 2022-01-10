using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace;

sealed class CommentConfiguration : DumpableObject, LinesAndSpaces.IConfiguration
{
    readonly LinesAndSpaces.IConfiguration Configuration;

    internal CommentConfiguration(LinesAndSpaces.IConfiguration configuration)
        => Configuration = configuration;

    int? LinesAndSpaces.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
    bool LinesAndSpaces.IConfiguration.IsPrefixLineComment => false;
    bool LinesAndSpaces.IConfiguration.IsSeparatorRequired => true;
    int LinesAndSpaces.IConfiguration.MinimalLineBreakCount => 0;
    SeparatorRequests LinesAndSpaces.IConfiguration.SeparatorRequests => Configuration.SeparatorRequests;
}