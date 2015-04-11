using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class NoReformat : DumpableObject, ISubConfiguration
    {
        public static readonly ISubConfiguration Instance = new NoReformat();

        NoReformat() { }

        string ISubConfiguration.Reformat(TokenItem target) => target.FullText;
        IConfiguration ISubConfiguration.Parent => DefaultFormat.Instance;
        string ISubConfiguration.ListItemHead => "";
    }
}