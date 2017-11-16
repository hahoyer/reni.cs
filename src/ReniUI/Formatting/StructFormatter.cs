using System.Collections.Generic;
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
            return structItem.GetEditPieces(targetPart);
        }
    }

}