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
            bool IsSeparatorRequired { get; }
        }

        readonly WhiteSpaceItem Target;
        readonly IConfiguration Configuration;

        [EnableDump]
        readonly LineGroup[] Lines;

        [EnableDump]
        readonly SpacesGroup[] Spaces;

        [EnableDump]
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

            var edits = new List<Edit>();
            edits.AddRange(Comments.SelectMany(
                (item, index) => item.GetEdits(indent, index != 0 || Configuration.IsSeparatorRequired)));

            if(Lines.Any())
            {
                var delta = Configuration.EmptyLineLimit ?? Lines.Length - Lines.Length;

                if(delta < 0)
                {
                    NotImplementedMethod(indent);
                    return default;
                }

                if(delta > 0)
                {
                    NotImplementedMethod(indent);
                    return default;
                }

                edits.AddRange(Lines.SelectMany(item => item.GetEdits()));
            }

            if(Spaces.Any())
            {
                NotImplementedMethod(indent);
                return default;
            }

            return edits;
        }
    }
}