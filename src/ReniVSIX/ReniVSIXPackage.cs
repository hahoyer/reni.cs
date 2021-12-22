﻿using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using ReniUI.Formatting;

namespace ReniVSIX
{
    /// <summary>
    ///     This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The minimum requirement for a class to be considered a valid package for Visual Studio
    ///         is to implement the IVsPackage interface and register itself with the shell.
    ///         This package uses the helper classes defined inside the Managed Package Framework (MPF)
    ///         to do it: it derives from the Package class that provides the implementation of the
    ///         IVsPackage interface and uses the registration attributes defined in the framework to
    ///         register itself and its components with the shell. These attributes tell the pkgdef creation
    ///         utility what data to put into .pkgdef file.
    ///     </para>
    ///     <para>
    ///         To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...
    ///         &gt; in .vsixmanifest file.
    ///     </para>
    /// </remarks>
    [ProvideService(typeof(ReniService), ServiceName = "Reni Language Service")]
    [ProvideLanguageService(typeof(ReniService), "Reni", 106)]
    [ProvideLanguageExtension(typeof(ReniService), ".reni")]
    [UsedImplicitly]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    public sealed class ReniVSIXPackage : AsyncPackage
    {
        /// <summary>
        ///     ReniVSIXPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "7e499f22-1bbb-4366-a9d7-89c1c01eac86";

        /// <summary>
        ///     Initialization of the package; this method is called right after the package is sited, so this is the place
        ///     where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A cancellation token to monitor for initialization cancellation, which can occur when
        ///     VS is shutting down.
        /// </param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>
        ///     A task representing the async work of package initialization, or an already completed task if there is none.
        ///     Do not return null from this method.
        /// </returns>
        protected override async System.Threading.Tasks.Task InitializeAsync
            (CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Initialize();
        }

        new void Initialize()
        {
            Tracer.Dumper.Configuration.Handlers.Add(typeof(ParseRequest),);
            var serviceContainer = this as IServiceContainer;
            var langService = new ReniService();
            langService.SetSite(this);
            serviceContainer.AddService(typeof(ReniService), langService, true);
        }

        internal IFormatter CreateFormattingProvider()
        {
            var pd = (Properties)GetDialogPage(typeof(Properties));
            return new ReniUI.Formatting.Configuration
            {
                MaxLineLength = pd.MaxLineLength //
                , EmptyLineLimit = pd.EmptyLineLimit
            }.Create();
        }
    }
}