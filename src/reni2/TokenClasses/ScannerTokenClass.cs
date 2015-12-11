using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using hw.Parser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class ScannerTokenClass : DumpableObject, IIconKeyProvider, Scanner<SourceSyntax>.IType, IUniqueIdProvider
    {
        string IIconKeyProvider.IconKey => "Symbol";
        ISubParser<SourceSyntax> Scanner<SourceSyntax>.IType.NextParser => this as ISubParser<SourceSyntax>;
        IType<SourceSyntax> Scanner<SourceSyntax>.IType.Type => this as IType<SourceSyntax>;
        string IUniqueIdProvider.Value => Id;
        public abstract string Id { get; }

        protected override string GetNodeDump() => Id;
    }
}