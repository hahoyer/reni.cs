using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxTree;

/// <summary>
///     Structured data, context free version
/// </summary>
sealed class CompoundSyntax : ValueSyntax
{
    static readonly bool NoFileDump = true;
    static bool IsInContainerDump;

    static readonly string RunId = Extension.GetFormattedNow() + "\n";
    static bool IsInsideFileDump;
    static int NextObjectId;

    [EnableDump(Order = 2)]
    [EnableDumpExcept(null)]
    internal readonly CleanupSyntax CleanupSection;

    [EnableDump(Order = 1)]
    internal readonly IStatementSyntax[] Statements;

    [DisableDump]
    public IEnumerable<FunctionSyntax> ConverterFunctions
        => Statements
            .Where(data => data.Declarer?.IsConverterSyntax ?? false)
            .Select(data => (FunctionSyntax)data.Value);

    [DisableDump]
    internal ValueSyntax[] PureStatements => Statements.Select(s => s.Value).ToArray();

    [EnableDump(Order = 100)]
    internal IDictionary<string, int> NameIndex
        => Statements
            .Select((statement, index) => (Key: statement.Declarer?.Name?.Value, Value: index))
            .Where(pair => pair.Key != null)
            .ToDictionary(item => item.Key, item => item.Value);

    [EnableDump(Order = 100)]
    internal int[] MutableDeclarations => GetIndexList(item => item.IsMutableSyntax).ToArray();

    [EnableDump(Order = 100)]
    internal int[] Converters => GetIndexList(item => item.IsConverterSyntax).ToArray();

    [EnableDump(Order = 100)]
    internal int[] MixInDeclarations => GetIndexList(item => item.IsMixInSyntax).ToArray();

    [DisableDump]
    internal int EndPosition => Statements.Length;

    [DisableDump]
    internal Size IndexSize => Size.GetAutoSize(Statements.Length);

    [DisableDump]
    internal string[] AllNames => Statements
        .Select(s => s.Declarer?.Name?.Value)
        .Where(name => name != null)
        .ToArray();

    [DisableDump]
    internal int[] ConverterStatementPositions
        => Statements
            .SelectMany((s, i) => s.Declarer.IsConverterSyntax? new[] { i } : new int[0])
            .ToArray();

    CompoundSyntax(IStatementSyntax[] statements, CleanupSyntax cleanupSection, Anchor anchor)
        : base(NextObjectId++, anchor)
    {
        Statements = statements;
        CleanupSection = cleanupSection;

        AssertValid();
    }

    [DisableDump]
    internal override bool? IsHollow => PureStatements.All(syntax => syntax.IsHollow == true);

    [DisableDump]
    protected override int DirectChildCount => Statements.Length + 1;

    public override string DumpData()
    {
        var isInsideFileDump = IsInsideFileDump;
        IsInsideFileDump = true;
        var result = NoFileDump || isInsideFileDump? DumpDataToString() : DumpDataToFile();
        IsInsideFileDump = isInsideFileDump;
        return result;
    }

    protected override string GetNodeDump()
        => GetType().PrettyName() + "(" + GetCompoundIdentificationDump() + ")";

    protected override Syntax GetDirectChild(int index)
    {
        if(index >= 0 && index < Statements.Length)
            return (Syntax)Statements[index];
        return index == Statements.Length? CleanupSection : null;
    }

    internal override Result GetResultForCache(ContextBase context, Category category)
        => context.GetCompound(this).GetResult(category);

    internal override ValueSyntax Visit(ISyntaxVisitor visitor)
    {
        var statements = Statements.Select(s => s.Value.Visit(visitor)).ToArray();
        var cleanupSection = CleanupSection?.Value.Visit(visitor);

        if(statements.All(s => s == null) && cleanupSection == null)
            return null;

        var newStatements = statements
            .Select((s, i) => s ?? Statements[i])
            .ToArray();

        var newCleanupSection
            = cleanupSection == null
                ? CleanupSection
                : new(cleanupSection, CleanupSection.Anchor);

        return Create(newStatements, newCleanupSection, Anchor);
    }

    internal override Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree listTarget = null) => this;

    public static CompoundSyntax Create(IStatementSyntax[] statements, CleanupSyntax cleanupSection, Anchor anchor)
        => new(statements, cleanupSection, anchor);

    public string GetCompoundIdentificationDump() => "." + ObjectId + "i";

    internal bool IsMutable(int position) => Statements[position].Declarer.IsMutableSyntax;

    internal int? Find(string name, bool publicOnly)
    {
        if(name == null)
            return null;

        return Statements
            .Select((data, index) => data.Declarer?.IsDefining(name, publicOnly) ?? false? index : (int?)null)
            .FirstOrDefault(data => data != null);
    }

    internal Result GetCleanup(ContextBase context, Category category)
    {
        if(CleanupSection != null && (category.HasCode() || category.HasClosures()))
            return context
                    .GetResult(category | Category.Type, CleanupSection.Value)
                    .GetConversion(Root.VoidType)
                    .GetLocalBlock(category)
                & category;

        return Root.VoidType.GetResult(category);
    }

    IEnumerable<int> GetIndexList(Func<DeclarerSyntax, bool> selector)
    {
        for(var index = 0; index < Statements.Length; index++)
        {
            var declarer = Statements[index].Declarer;
            if(declarer != null && selector(declarer))
                yield return index;
        }
    }

    string DumpDataToFile()
    {
        var dumpFile = ("compound." + ObjectId).ToSmbFile();
        var oldResult = dumpFile.String;
        var newResult = (RunId + DumpDataToString()).Replace("\n", "\r\n");
        if(oldResult == null || !oldResult.StartsWith(RunId))
        {
            oldResult = newResult;
            dumpFile.String = oldResult;
        }
        else
            (oldResult == newResult).Assert();

        return Tracer.FilePosition(dumpFile.FullName, 1, 0, FilePositionTag.Debug) + "see there\n";
    }

    string DumpDataToString()
    {
        var isInDump = IsInContainerDump;
        IsInContainerDump = true;
        var result = base.DumpData();
        IsInContainerDump = isInDump;
        return result;
    }
}

interface IStatementSyntax
{
    ValueSyntax Value { get; }
    DeclarerSyntax Declarer { get; }
    SourcePart SourcePart { get; }
    ValueSyntax ToValueSyntax(Anchor anchor);
    IStatementSyntax With(Anchor frameItems);
}