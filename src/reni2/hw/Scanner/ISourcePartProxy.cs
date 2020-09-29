using System;

namespace hw.Scanner
{
    /// <summary>
    ///     Proxy for <see cref="SourcePart" />.
    /// </summary>
    [Obsolete("interface will be removed")]
    public interface ISourcePartProxy
    {
        SourcePart All { get; }
    }
}