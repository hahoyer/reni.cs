using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;
using Reni.Runtime;
using Reni.Struct;

namespace Reni.Code;

static class Generator
{
    internal static readonly CSharpCodeProvider Provider = new();

    internal static string MainFunctionName => "MainFunction";

    internal static string FunctionName(FunctionId functionId)
        => (functionId.IsGetter? "GetFunction" : "SetFunction") + functionId.Index;

    internal static string CreateCSharpString
    (
        this string moduleName,
        Container main,
        FunctionCache<int, FunctionContainer> functions
    )
        => new CSharp_Generated(moduleName, main, functions).TransformText();

    internal static void CodeToFile(string name, string result, bool traceFilePosn)
    {
        var streamWriter = new StreamWriter(name);
        if(traceFilePosn)
            Tracer
                .FilePosition(name.ToSmbFile().FullName, 0, 0, FilePositionTag.Debug)
                .Log();
        streamWriter.Write(result);
        streamWriter.Close();
    }

    internal static Assembly CodeToAssembly
        (this string cSharpString, bool traceFilePosition, bool includeDebugInformation)
    {
        var directoryName
            = Environment.GetEnvironmentVariable
                ("temp")
            + "\\reni.compiler\\"
            + Process.GetCurrentProcess().Id
            + "."
            + Thread.CurrentThread.ManagedThreadId;
        directoryName.ToSmbFile().EnsureIsExistentDirectory();
        var name = directoryName + ".reni.cs";
        name.ToSmbFile().CheckedEnsureDirectoryOfFileExists();

        CodeToFile(name, cSharpString, traceFilePosition);

        MetadataReferenceResolver resolver = new MyResolver();
        var cp = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithAllowUnsafe(true)
            .WithPlatform(Platform.X64)
            ;

        System.Type[] types =
        [
            typeof(Generator)
            , typeof(SmbFile)
            , typeof(Data)
            , typeof(object)
        ];

        var runtime = Assembly
            .GetExecutingAssembly()
            .GetAssemblies()
            .Top( a=>a.FullName.AssertNotNull().Contains("System.Runtime"));

        var referencedAssemblies
            = types
                .Select(type => Assembly.GetAssembly(type).AssertNotNull().Location)
                .Union([runtime.Location])
                .Distinct()
                .Select(location => MetadataReference.CreateFromFile(location))
                .ToArray();

        var cr = CSharpCompilation.Create(Guid.NewGuid().ToString()
            , [CSharpSyntaxTree.ParseText(cSharpString)]
            , referencedAssemblies
            , cp
        );

        using var ms = new MemoryStream();
        var emitResult = cr.Emit(ms);

        if(!emitResult.Success)
        {
            var diagnostics = emitResult.Diagnostics.ToArray();
            HandleErrors(diagnostics);
            throw new CSharpCompilerErrorException(diagnostics);
        }

        if(!includeDebugInformation)
            directoryName.ToSmbFile().Delete(true);

        ms.Seek(0, SeekOrigin.Begin);

        return AssemblyLoadContext.Default.LoadFromStream(ms);
    }

    internal static void HandleErrors(Diagnostic[] diagnostics)
    {
        foreach(var diagnostic in diagnostics)
            diagnostic.ToString().Log();
    }

    static TValue[] T<TValue>(params TValue[] value) => value;
}

sealed class MyResolver : MetadataReferenceResolver
{
    public override bool Equals(object other) => other == this;
    public override int GetHashCode() => 1;

    public override ImmutableArray<PortableExecutableReference> ResolveReference
        (string reference, string baseFilePath, MetadataReferenceProperties properties)
    {
        Dumpable.NotImplementedFunction(reference, baseFilePath, properties);
        return default;
    }
}

sealed class CSharpCompilerErrorException : Exception
{
    public string[] Errors { get; }

    public CSharpCompilerErrorException(Diagnostic[] diagnostics)
    {
        var errors = new List<string>();
        foreach(var diagnostic in diagnostics)
            errors.Add(diagnostic.ToString());

        Errors = errors.ToArray();
    }
}

// ReSharper disable InconsistentNaming
partial class CSharp_Generated
// ReSharper restore InconsistentNaming
{
    readonly FunctionCache<int, FunctionContainer> Functions;
    readonly Container Main;
    readonly string ModuleName;

    internal CSharp_Generated
        (string moduleName, Container main, FunctionCache<int, FunctionContainer> functions)
    {
        ModuleName = moduleName;
        Main = main;
        Functions = functions;
    }
}