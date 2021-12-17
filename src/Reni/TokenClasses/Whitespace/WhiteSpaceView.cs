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

        [EnableDump]
        readonly LinesAndSpaces LinesAndSpaces;

        internal WhiteSpaceView(WhiteSpaceItem target, IConfiguration configuration)
        {
            Target = target;
            Configuration = configuration;
            (Comments, LinesAndSpaces) = target.Items.Any()
                ?CommentGroup.Create(target.Items, configuration)
                : CommentGroup.Create(target.SourcePart.Start, configuration);
        }

        protected override string GetNodeDump() => Target.SourcePart.NodeDump + " " + base.GetNodeDump();

        internal IEnumerable<Edit> GetEdits(int indent)
        {
            var commentEdits = Comments
                .SelectMany((item, index) => item.GetEdits(IsSeparatorRequired(index), indent))
                .ToArray();

            var isSeparatorRequired =
                Configuration.MinimalLineBreakCount == 0 &&
                (
                    Comments.Any()
                        ? Configuration.SeparatorRequests.Tail && Comments.Last().IsSeparatorRequired
                        : Configuration.SeparatorRequests.Flat
                );


            var linesAndSpacesEdits
                = LinesAndSpaces.GetEdits(isSeparatorRequired, indent).ToArray();

            return T(commentEdits, linesAndSpacesEdits).ConcatMany();
        }

        bool IsSeparatorRequired(int index)
            => index == 0
                ? Configuration.SeparatorRequests.Head
                : Configuration.SeparatorRequests.Inner;
    }
}