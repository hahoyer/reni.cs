using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Parser
{
    internal interface ITokenClass
    {
        string Name { set; }
        bool IsEnd { get; }
        string PrioTableName(string name);
        ITokenFactory NewTokenFactory { get; }
        IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right);
    }
}