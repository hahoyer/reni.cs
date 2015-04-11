using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class DefaultSubFormat : DumpableObject, ISubConfiguration
    {
        readonly DefaultFormat _parent;

        internal DefaultSubFormat(DefaultFormat parent) { _parent = parent; }

        IConfiguration ISubConfiguration.Parent => _parent;

        string ISubConfiguration.Reformat(TokenItem target)
        {
            if(target.HeadComments == "" && target.TailComments == "")
                return target.Id;

            NotImplementedMethod(target);
            return null;
        }

        string ISubConfiguration.ListItemHead => "";
    }
}