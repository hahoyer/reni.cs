using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting;

sealed class FormatterByBinaryTree : DumpableObject, IFormatter
{
    readonly Configuration Configuration;

    public FormatterByBinaryTree(Configuration configuration) => Configuration = configuration;

    IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compilerBrowser, SourcePart targetPart)
    {
        if(compilerBrowser.IsTooSmall(targetPart))
            return new Edit[0];

        var item = BinaryTreeProxy.Create(compilerBrowser.Compiler.BinaryTree, Configuration);
        var trace = DateTime.Today.Year < 2020;
        item.SetupPositions();
        if(trace) item.LogDump().Log(FilePositionTag.Debug);
        var sourcePartEdits = item.Edits.ToArray();
        var editPieces
            = sourcePartEdits
                .GetEditPieces()
                .Where(editPiece => IsRelevant(editPiece, targetPart))
                .ToArray();
        if(trace) editPieces.LogDump().Log(FilePositionTag.Debug);
        return editPieces;
    }

    static bool IsRelevant(Edit editPiece, SourcePart targetPart)
    {
        if(targetPart == null)
            return true;
        var sourcePart = editPiece.Remove;
        return targetPart.Source == sourcePart.Source &&
            targetPart.Position <= sourcePart.EndPosition &&
            sourcePart.Position <= targetPart.EndPosition;
    }
}