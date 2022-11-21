using hw.DebugFormatter;

namespace Reni.DeclarationOptions;

sealed class Declaration : DumpableObject
{
    internal readonly string Name;

    internal Declaration(string name) => Name = name;

    internal bool IsMatch(string searchId) => Name.StartsWith(searchId);
}