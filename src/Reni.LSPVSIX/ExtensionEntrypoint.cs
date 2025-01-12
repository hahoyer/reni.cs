using Microsoft.VisualStudio.Extensibility;

namespace Reni.LSPVSIX;

[UsedImplicitly]
[VisualStudioContribution]
class ExtensionEntrypoint : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            "Reni.LSPVSIX",
            ExtensionAssemblyVersion,
            "Harald Hoyer",
            "Reni.LSPVSIX",
            "Reni Language support")
        {
            Icon = "reni.ico"
        }
    };
}