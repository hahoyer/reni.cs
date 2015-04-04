using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class SourceWrapper : Microsoft.VisualStudio.Package.Source
    {
        readonly Source _data;

        public SourceWrapper(IVsTextLines buffer, ReniService parent, Source source)
            : base(parent, buffer, null)
        {
            _data = source;
        }

        public override TokenInfo GetTokenInfo(int line, int col) => _data.GetTokenInfo(line, col);
    }
}