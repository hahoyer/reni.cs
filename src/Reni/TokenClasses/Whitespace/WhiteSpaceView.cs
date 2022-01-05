using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.TokenClasses.Whitespace
{
    sealed class WhiteSpaceView : DumpableObject
    {
        readonly WhiteSpaceItem Target;

        [EnableDump]
        readonly CommentGroup[] Comments;

        [EnableDump]
        readonly LinesAndSpaces LinesAndSpaces;

        internal WhiteSpaceView(WhiteSpaceItem target, LineGroup.IConfiguration  configuration)
        {
            Target = target;
            (Comments, LinesAndSpaces) = target.Items.Any()
                ? CommentGroup.Create(target.Items, configuration)
                : CommentGroup.Create(target.SourcePart.Start, configuration);
        }

        protected override string GetNodeDump() => Target.SourcePart.NodeDump + " " + base.GetNodeDump();

        internal IEnumerable<Edit> GetEdits(int indent)
        {
            var commentEdits = Comments
                .SelectMany((item, index) => item.GetEdits(indent))
                .ToArray();

            var linesAndSpacesEdits
                = LinesAndSpaces.GetEdits(indent).ToArray();

            return T(commentEdits, linesAndSpacesEdits).ConcatMany();
        }
    }
}