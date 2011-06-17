using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.ReniParser;

namespace Reni.Syntax
{
    /// <summary>
    ///     Mamber name and in case of function call an argument list
    /// </summary>
    internal sealed class MemberElem : ReniObject
    {
        private static int _nextObjectId;
        internal readonly ICompileSyntax Args;
        internal readonly DefineableToken DefineableToken;

        public MemberElem(DefineableToken defineableToken, ICompileSyntax args)
            : base(_nextObjectId++)
        {
            DefineableToken = defineableToken;
            Args = args;
            //StopByObjectId(-10);
        }

        public string FilePosition()
        {
            if(DefineableToken != null)
                return DefineableToken.Data.FilePosition;
            return Args.FilePosition();
        }

        internal override string DumpShort()
        {
            var result = "";
            if(DefineableToken != null)
                result = DefineableToken.Data.Name;
            if(DefineableToken != null && Args != null)
                result += "(";
            if(Args != null)
                result += Args.DumpShort();
            if(DefineableToken != null && Args != null)
                result += ")";
            return result;
        }
    }
}