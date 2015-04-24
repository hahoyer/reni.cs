using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Package;
using Reni.UserInterface;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ParseRequestWrapper : DumpableObject
    {
        readonly ParseRequest _parent;
        readonly Source _source;
        readonly ValueCache<TokenInformation> _currenCache;

        internal ParseRequestWrapper(ParseRequest parent)
        {
            _parent = parent;
            _source = new Source(_parent.Text ?? "");
            _currenCache = new ValueCache<TokenInformation>(GetCurrentForCache);
        }

        TokenInformation GetCurrentForCache()
            => _source
                .FromLineAndColumn(_parent.Line, _parent.Col);

        TokenInformation Current => _currenCache.Value;

        internal void Execute()
        {
            ScanBraces();
            //ScanErrors();
        }

        void ScanBraces()
        {
            if(!_parent.Sink.BraceMatching)
                return;

            var result = Braces.Select(part => part.ToTextSpan()).ToArray();
            if(result.Length > 1)
                _parent.Sink.MatchMultiple(result, 0);
        }

        void ScanErrors()
        {
            if (!_parent.Reason.In( ParseReason.HighlightBraces,ParseReason.MatchBraces,ParseReason.MemberSelectAndHighlightBraces))
                return;

            NotImplementedMethod();
            
        }

        IEnumerable<SourcePart> Braces => _source.BracesLike(Current);
    }
}