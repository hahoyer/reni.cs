using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace Stx.CodeItems
{
    abstract class CodeItem : DumpableObject
    {
        public const int PointerByteSize = 4;

        public static IEnumerable<CodeItem> Combine
            (CodeItem[] left, CodeItem center, CodeItem[] right, CodeItem tail = null)
        {
            var items = new List<CodeItem>();
            if(left != null)
                items.AddRange(left);
            if(center != null)
                items.Add(center);
            if(right != null)
                items.AddRange(right);
            if(tail != null)
                items.Add(tail);

            return items;
        }

        public static CodeItem CreateSourceHint(IToken token) => new SourceHint(token);
        public static CodeItem CreateReassign(int byteSize) => new Reassign(byteSize);
        public static CodeItem CreateAccessVariable(string name) => new AccessVariable(name);

        public static CodeItem CreateAccessVariableValue
            (string name, int byteSize) => new AccessVariableValue(name, byteSize);

        public static CodeItem Error(IssueId issueId) => new Error(issueId);
        public static CodeItem CreateDereference(int targetSize) => new Dereferenced(targetSize);

        public abstract int InByteSize {get;}
        public abstract int OutByteSize {get;}
        public int ByteSize => OutByteSize - InByteSize;
    }

    sealed class Dereferenced : CodeItem
    {
        public Dereferenced(int targetSize) => OutByteSize = targetSize;

        public override int InByteSize => PointerByteSize;
        public override int OutByteSize {get;}
    }

    sealed class Error : CodeItem
    {
        readonly IssueId IssueId;
        public Error(IssueId issueId) => IssueId = issueId;
        public override int InByteSize => 0;
        public override int OutByteSize => 0;
    }

    sealed class AccessVariable : CodeItem
    {
        string Name;
        public AccessVariable(string name) => Name = name;
        public override int InByteSize => 0;
        public override int OutByteSize => PointerByteSize;
    }

    sealed class AccessVariableValue : CodeItem
    {
        string Name;

        public AccessVariableValue(string name, int byteSize)
        {
            OutByteSize = byteSize;
            Name = name;
        }
        public override int InByteSize => 0;
        public override int OutByteSize {get;}
    }

    sealed class Reassign : CodeItem
    {
        readonly int TargetByteSize;
        public Reassign(int byteSize) => TargetByteSize = byteSize;
        public override int InByteSize => PointerByteSize + TargetByteSize;
        public override int OutByteSize => 0;

    }

    sealed class SourceHint : CodeItem
    {
        readonly IToken Token;
        public SourceHint(IToken token) => Token = token;
        public override int InByteSize => 0;
        public override int OutByteSize => 0;
    }


    static class Extension
    {
        public static int GetByteSize(this IEnumerable<CodeItem> target) => target.Sum(i => i.ByteSize);
    }
}