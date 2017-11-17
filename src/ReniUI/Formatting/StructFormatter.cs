using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public class StructFormatter : DumpableObject, IFormatter
    {
        readonly Configuration Configuration;
        public StructFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {
            var structItem = compiler.Locate(targetPart).CreateStruct();

            var sourcePartEdits = structItem.GetSourcePartEdits(targetPart);
            var editPieces = sourcePartEdits.GetEditPieces(targetPart);
            return editPieces;
        }
    }

    class SourcePartEdit
    {
        public SourcePartEdit(SourcePart sourcePart, string newText = null)
        {
            
        }
    }

    static class SourcePartEditExtension
    {
        public static IEnumerable<Edit> GetEditPieces(this IEnumerable<SourcePartEdit> target, SourcePart targetPart)
        {
            Dumpable.NotImplementedFunction(target.ToArray(),targetPart);
            return null;
        }
        
    }

}