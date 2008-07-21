using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Syntax
{
    internal interface ICompileSyntax
    {
        Result Result(ContextBase context, Category category);
        string DumpShort();
        string FilePosition();
        void AddToCache(ContextBase context, CacheItem cacheItem);
    }
}