using hw.Scanner;

namespace ReniUI.Formatting
{
    interface ISourcePartEdit
    {
        bool HasLines {get;}
        SourcePart SourcePart { get; }
        ISourcePartEdit Indent(int count);
        ISourcePartEdit AddLineBreaks(int count);
    }
}                                                                             