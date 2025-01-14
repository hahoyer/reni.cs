namespace Reni.Code;

/// <summary>
///     Nothing, since void cannot be used for this purpose
/// </summary>
class None
{
    [PublicAPI]
    internal static readonly None Instance = new None();
}