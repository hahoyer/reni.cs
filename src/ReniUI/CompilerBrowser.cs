using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Helper;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace ReniUI;

public sealed class CompilerBrowser : DumpableObject, ValueCache.IContainer
{
    readonly ValueCache<Compiler> ParentCache;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly PositionDictionary<Helper.Syntax> PositionDictionary = new();

    readonly ValueCache<Helper.Syntax> SyntaxCache;

    CompilerBrowser(Func<Compiler> parent)
    {
        ParentCache = new(parent);
        SyntaxCache = new(GetSyntax);
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    public Source Source => Compiler.Source;

    internal Compiler Compiler => ParentCache.Value;

    public StringStream Result
    {
        get
        {
            var result = new StringStream();
            Compiler.Parameters.OutStream = result;
            Compiler.Execute();
            return result;
        }
    }

    internal IExecutionContext ExecutionContext => Compiler;
    public BinaryTree LeftMost => Compiler.BinaryTree.LeftMost;

    internal IEnumerable<Issue> Issues => Compiler.Issues;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal Helper.Syntax Syntax
    {
        get
        {
            if(IsInDump && !SyntaxCache.IsValid)
                return null;
            return SyntaxCache.Value;
        }
    }

    public static CompilerBrowser FromText
        (string text, CompilerParameters parameters, string sourceIdentifier = null)
        => new(() => Compiler.FromText(text, parameters, sourceIdentifier));

    public static CompilerBrowser FromText(string text, string sourceIdentifier = null)
        => new(() => Compiler.FromText(text, null, sourceIdentifier));

    public static CompilerBrowser FromFile(string fileName, CompilerParameters parameters = null)
        => new(() => Compiler.FromFile(fileName, parameters));

    public Item Locate(SourcePosition offset)
    {
        SyntaxCache.IsValid = true;
        return Item.LocateByPosition(Compiler.BinaryTree, offset);
    }

    public IEnumerable<Item> Locate(int start, int end)
    {
        var current = start;
        do
        {
            var result = Locate(current);
            yield return result;
            if(result.SourcePart.Length > 0)
                current = result.EndPosition;
            else
                current++;
        }
        while(current < end);
    }

    public string FlatFormat(bool areEmptyLinesPossible)
        => Compiler.BinaryTree.GetFlatString(areEmptyLinesPossible);

    public Item Locate(int offset) => Locate(Source + offset);

    internal BinaryTree Locate(SourcePart span)
        => Item.Locate(Compiler.BinaryTree, span);

    internal FunctionType Function(int index)
        => Compiler.Root.Function(index);


    internal string Reformat(IFormatter formatter = null, SourcePart targetPart = null)
        => (formatter ?? new Formatting.Configuration().Create())
            .GetEditPieces(this, targetPart)
            .Combine(Compiler.Source.All);

    internal void Ensure() => Compiler.Execute();

    internal void Execute(DataStack dataStack) => Compiler.ExecuteFromCode(dataStack);

    internal IEnumerable<Edit> GetEditPieces(SourcePart sourcePart, IFormatter formatter = null)
        => (formatter ?? new Formatting.Configuration().Create())
            .GetEditPieces(this, sourcePart);

    Helper.Syntax GetSyntax()
    {
        try
        {
            if(IsInDump)
                return null;
            var trace = Debugger.IsAttached && DateTime.Today.Year < 2020;

            var compilerSyntax = Compiler.Syntax;
            if(trace)
            {
                compilerSyntax.Dump().FlaggedLine();
                compilerSyntax.Anchor.Dump().FlaggedLine();
            }

            var syntax = new Helper.Syntax(compilerSyntax, PositionDictionary);

            syntax.GetNodesFromLeftToRight().ToArray().AssertNotNull();
            PositionDictionary.AssertValid(Compiler.BinaryTree);
            if(trace)
                syntax.Dump().FlaggedLine();
            return syntax;
        }
        catch(Exception e)
        {
            $"Syntax: Unexpected {e} \nText:\n{Source.Data}".Log();
            throw;
        }
    }

    internal string[] DeclarationOptions(int offset)
    {
        NotImplementedMethod(offset);
        return default;
    }

    internal(string Text, SourcePart Span ) GetDataTipText(int line, int column)
    {
        NotImplementedMethod(line, column);
        return default;
    }
}