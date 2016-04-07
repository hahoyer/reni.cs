using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

using hw.Helper;
using hw.Parser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class ScannerTokenClass : DumpableObject, Scanner<Syntax>.IType, IUniqueIdProvider
    {
        ISubParser<Syntax> Scanner<Syntax>.IType.NextParser => this as ISubParser<Syntax>;
        IType<Syntax> Scanner<Syntax>.IType.Type => this as IType<Syntax>;
        string IUniqueIdProvider.Value => Id;
        public abstract string Id { get; }

        protected override string GetNodeDump() => Id;
    }
}