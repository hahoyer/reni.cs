using System;
using System.ComponentModel.Design;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Text.Editor;

namespace ReniVSIX;

sealed class Main : DumpableObject
{
    internal static readonly Main Instance = new();

    [DisableDump]
    internal IEditorOptions EditorOptions;

    [DisableDump]
    ReniService ReniServiceCache;

    [DisableDump]
    internal ReniService ReniService => ReniServiceCache.AssertNotNull();

    internal void RegisterPackage(ReniVSIXPackage package)
    {
        ReniServiceCache.AssertIsNull();
        Dumper.Register();
        var serviceContainer = package as IServiceContainer;
        ReniServiceCache = new();
        ReniServiceCache.SetSite(package);
        serviceContainer.AddService(typeof(ReniService), ReniServiceCache, true);
    }

    internal void GetOptions(Func<IEditorOptions> getOptions) => EditorOptions = getOptions();
    //var o1 = Filter(editorOptions);
    //var g1 = Filter(editorOptions.GlobalOptions);
}