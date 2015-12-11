using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.Formatting
{
    sealed class Item : DumpableObject
    {
        internal readonly string WhiteSpaces;
        internal readonly IToken Token;

        internal Item(string whiteSpaces, IToken token = null)
        {
            Token = token;
            WhiteSpaces = whiteSpaces;
            //Tracer.ConditionalBreak(Id == ";");
        }

        internal string Id => WhiteSpaces + (Token?.Id ?? "");
        internal int Length => Id.Length;

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Id.Quote();

        internal string Filter(SourcePart targetPart)
            => FilterPrefix(targetPart) + FilterToken(targetPart);

        string FilterToken(SourcePart targetPart)
            => Token.Characters.Intersect(targetPart)?.Id ?? "";

        string FilterPrefix(SourcePart targetPart)
        {
            var sourcePart = Token.PrefixCharacters();
            var toUse = targetPart.Intersect(sourcePart);
            if(toUse == null)
                return "";

            var result = WhiteSpaces;
            var length = result.Length + toUse.Length - sourcePart.Length;
            return length <= 0
                ? ""
                : result.Substring(toUse.Position - sourcePart.Position, length);

        }

    }
}