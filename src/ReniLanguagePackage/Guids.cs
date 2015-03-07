// Guids.cs
// MUST match guids.h
using System;

namespace HoyerWare.ReniLanguagePackage
{
    static class GuidList
    {
        public const string GuidReniLanguagePackagePkgString = "c5d6fc9a-13f2-4379-b811-3393a68442bf";
        public const string GuidReniLanguagePackageCmdSetString = "d478f938-43d0-46af-bdee-f80943c881fa";

        public static readonly Guid GuidReniLanguagePackageCmdSet = new Guid(GuidReniLanguagePackageCmdSetString);
    };
}