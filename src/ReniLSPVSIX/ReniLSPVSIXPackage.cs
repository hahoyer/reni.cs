using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;

namespace ReniLSPVSIX;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid("99058c20-8716-4632-a0a5-60de0d6ca628")]
public sealed class ReniLSPVSIXPackage : AsyncPackage
{
    [PublicAPI]
    protected override async Task InitializeAsync
        (CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        => await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
}