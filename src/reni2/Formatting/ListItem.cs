using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class ListItem : DumpableObject
    {
        internal readonly ITreeItem Target;
        internal readonly TokenItem Token;

        internal ListItem(ITreeItem target, TokenItem token)
        {
            Target = target;
            Token = token;
        }

        internal int Length => (Target?.Length ?? 0) + (Token?.Length ?? 0);

        public IAssessment Assess(IAssessor assessor)
        {
            if(Token == null)
                return Target.Assess(assessor);
            var result = assessor.Assess(Token);
            return result.IsMaximal ? result : Target.Assess(assessor).plus(result);
        }

        internal string Reformat(ISubConfiguration configuration)
        {
            var head = configuration.ListItemHead;
            var target = configuration.Parent.Reformat(Target);
            var token = Token == null ? "" : configuration.Reformat(Token);
            return head + target + token;
        }
    }
}