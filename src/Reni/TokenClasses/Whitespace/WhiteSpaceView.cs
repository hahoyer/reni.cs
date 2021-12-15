using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace
{
    class WhiteSpaceView : DumpableObject
    {
        internal interface IConfiguration : CommentGroup.IConfiguration
        {
            new int MinimalLineBreakCount { get; }
            new int? EmptyLineLimit { get; }
            bool IsSeparatorRequired{ get; }
        }

        readonly WhiteSpaceItem Target;
        readonly IConfiguration Configuration;

        readonly LineGroup[] Lines;
        readonly SpacesGroup[] Spaces;
        readonly CommentGroup[] Comments;

        internal WhiteSpaceView(WhiteSpaceItem target, IConfiguration configuration)
        {
            Target = target;
            Configuration = configuration;
            (Comments, (Lines, Spaces)) = CommentGroup.Create(target.Items, configuration);
        }

        protected override string GetNodeDump() => Target.SourcePart.NodeDump + " " + base.GetNodeDump();

        internal IEnumerable<Edit> GetEdits(int indent)
        {
            (indent == 0).Assert();

            foreach(var edit in Comments.SelectMany((comment,index)=>comment.GetEdits(indent, index != 0 || Configuration.IsSeparatorRequired)))
                yield return edit;

            if(Lines.Any())
            {
                NotImplementedMethod(indent);
                yield break;
            }
            if(Spaces.Any())
            {
                NotImplementedMethod(indent);
                yield break;
            }
        }
    }
}