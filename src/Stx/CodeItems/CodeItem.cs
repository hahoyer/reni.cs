using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace Stx.CodeItems
{
    abstract class CodeItem : DumpableObject
    {
        public const int ArrayIndexByteSize = 4;
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

        public static CodeItem CreateArrayAccessVariable(string name, int itemByteSize)
            => new ArrayAccessVariable(name, itemByteSize);

        public static CodeItem CreateDrop(int size) => new Drop(size);

        [EnableDumpExcept(0)]
        protected abstract int InByteSize {get;}

        [EnableDumpExcept(0)]
        protected abstract int OutByteSize {get;}

        public int ByteSize => OutByteSize - InByteSize;
    }

    sealed class Drop : CodeItem
    {
        public Drop(int size) => InByteSize = size;

        [DisableDump]
        protected override int InByteSize {get;}

        [DisableDump]
        protected override int OutByteSize => 0;
    }

    sealed class ArrayAccessVariable : CodeItem
    {
        readonly int ItemByteSize;

        [EnableDump]
        readonly string Name;

        public ArrayAccessVariable(string name, int itemByteSize)
        {
            Name = name;
            ItemByteSize = itemByteSize;
        }

        [DisableDump]
        protected override int InByteSize => ArrayIndexByteSize + PointerByteSize;

        [DisableDump]
        protected override int OutByteSize => PointerByteSize;
    }

    sealed class Dereferenced : CodeItem
    {
        public Dereferenced(int targetSize) => OutByteSize = targetSize;

        [DisableDump]
        protected override int InByteSize => PointerByteSize;

        protected override int OutByteSize {get;}
    }

    sealed class Error : CodeItem
    {
        [EnableDump]
        readonly IssueId IssueId;

        public Error(IssueId issueId) => IssueId = issueId;
        protected override int InByteSize => 0;
        protected override int OutByteSize => 0;
    }

    sealed class AccessVariable : CodeItem
    {
        [EnableDump]
        string Name;

        public AccessVariable(string name) => Name = name;
        protected override int InByteSize => 0;
        protected override int OutByteSize => PointerByteSize;
    }

    sealed class AccessVariableValue : CodeItem
    {
        [EnableDump]
        string Name;

        public AccessVariableValue(string name, int byteSize)
        {
            OutByteSize = byteSize;
            Name = name;
        }

        protected override int InByteSize => 0;
        protected override int OutByteSize {get;}
    }

    sealed class Reassign : CodeItem
    {
        [EnableDump]
        readonly int TargetByteSize;

        public Reassign(int byteSize) => TargetByteSize = byteSize;

        [DisableDump]
        protected override int InByteSize => PointerByteSize + TargetByteSize;

        protected override int OutByteSize => 0;
    }

    sealed class SourceHint : CodeItem
    {
        readonly IToken Token;
        public SourceHint(IToken token) => Token = token;
        protected override int InByteSize => 0;
        protected override int OutByteSize => 0;
    }


    static class Extension
    {
        public static int GetByteSize(this IEnumerable<CodeItem> target) => target.Sum(i => i.ByteSize);

        public static CodeItem[] Aggregate(this IEnumerable<CodeItem[]> target)
        {
            Tracer.ConditionalBreak(Tracer.Dump(target.ToArray()));
            return null;
        }
    }
}