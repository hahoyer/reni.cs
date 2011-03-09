using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Parser
{
    internal interface ITokenFactory
    {
        ITokenClass TokenClass(string name);
        ParserInst Parser { get; }
        PrioTable PrioTable { get; }
        ITokenClass ListClass { get; }
        ITokenClass RightParentethesisClass(int level);
        ITokenClass LeftParentethesisClass(int level);
    }
}