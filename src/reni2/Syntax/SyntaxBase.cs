using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    internal interface ICompileSyntax
    {
        Result Result(ContextBase context, Category category);
        string DumpShort();
        string FilePosition();
        void AddToCacheForDebug(ContextBase context, object cacheItem);
        int ObjectId{ get;}
        Token FirstToken { get; }
        Token LastToken { get; }
    }
}