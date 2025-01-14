namespace Reni;

/// <summary>
///     Parameters for compilation
/// </summary>
[PublicAPI]
public sealed class CompilerParameters : Attribute
{
    public sealed class TraceOptionsClass
    {
        [Node]
        [EnableDump]
        public bool CodeSequence;

        [Node]
        [EnableDump]
        public bool ExecutedCode;

        [Node]
        [EnableDump]
        public bool Functions;

        [Node]
        [EnableDump]
        public bool GeneratorFilePosition;

        [Node]
        [EnableDump]
        public bool Parser;

        public void None()
        {
            Parser = false;
            CodeSequence = false;
            ExecutedCode = false;
            Functions = false;
        }

        public void UseOnModeErrorFocus()
        {
            CodeSequence = true;
            ExecutedCode = true;
            Functions = true;
            GeneratorFilePosition = true;
        }
    }

    /// <summary>
    ///     Shows or hides syntax tree
    /// </summary>
    [Node]
    [EnableDump]
    public readonly TraceOptionsClass TraceOptions = new();

    public CompilationLevel CompilationLevel = CompilationLevel.Run;
    public bool? DebuggableGeneratedCode = false;

    public IOutStream? OutStream;

    [Node]
    [EnableDump]
    public bool ProcessErrors;

    public bool RunFromCode;
    public bool Semantics;

    public bool IsParserRequired => CompilationLevel >= CompilationLevel.Parser || Semantics;
    public bool IsSyntaxRequired => CompilationLevel >= CompilationLevel.Syntax || Semantics;
    public bool IsCodeRequired => CompilationLevel >= CompilationLevel.Code;
    public bool IsRunRequired => CompilationLevel >= CompilationLevel.Run;

    public bool ParseOnly
    {
        set
        {
            value.Assert();
            CompilationLevel = CompilationLevel.Scanner;
        }
    }

    internal static CompilerParameters CreateTraceAll()
    {
        var result = new CompilerParameters();
        result.TraceOptions.UseOnModeErrorFocus();
        return result;
    }
}

public enum CompilationLevel
{
    Scanner = 0
    , Parser = 1
    , Syntax = 2
    , Code = 3
    , Run = 4
}

public sealed class NodeAttribute : Attribute;