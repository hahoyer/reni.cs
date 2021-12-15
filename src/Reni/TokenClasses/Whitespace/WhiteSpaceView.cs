using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.TokenClasses.Whitespace
{
    class WhiteSpaceView : DumpableObject
    {
        internal interface IConfiguration : CommentGroup.IConfiguration { }

        readonly WhiteSpaceItem Target;
        readonly IConfiguration Configuration;

        [EnableDump]
        readonly CommentGroup[] Comments;
        readonly LinesAndSpaces LinesAndSpaces;

        internal WhiteSpaceView(WhiteSpaceItem target, IConfiguration configuration)
        {
            Target = target;
            Configuration = configuration;
            (Comments, LinesAndSpaces) = CommentGroup.Create(target.Items, configuration);
        }

        protected override string GetNodeDump() => Target.SourcePart.NodeDump + " " + base.GetNodeDump();

        internal IEnumerable<Edit> GetEdits(int indent)
        {
            (indent == 0).Assert();

            var commentEdits = Comments
                .SelectMany((item, index) => item.GetEdits(indent, IsSeparatorRequired(index)))
                .ToArray();

            var isSeparatorRequired = Configuration.SeparatorRequests.Tail && (Comments.LastOrDefault()?.IsSeparatorRequired??false);
            var linesAndSpacesEdits = LinesAndSpaces.GetEdits(indent, isSeparatorRequired, Target.SourcePart.End).ToArray();

            return T(commentEdits, linesAndSpacesEdits).ConcatMany();
        }

        bool IsSeparatorRequired(int index)
            => index == 0? Configuration.SeparatorRequests.Head : Configuration.SeparatorRequests.Inner;
    }
}