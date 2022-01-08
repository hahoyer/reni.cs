using hw.DebugFormatter;

namespace ReniUI.Formatting;

sealed class EditPieceParameter : DumpableObject, IEditPiecesConfiguration
{
    int Indent;

    int IEditPiecesConfiguration.Indent
    {
        get => Indent;
        set => Indent = value;
    }

    protected override string GetNodeDump()
    {
        var result
            = $"{(Indent > 0? Indent + ">>" : "")}" +
            $"{(Indent < 0? "<<" + Indent : "")}";
        return result == ""? base.GetNodeDump() : result;
    }

    internal int GetIndentCharacterCount(int indentCount)
        => Indent > 0? Indent * indentCount : 0;
}