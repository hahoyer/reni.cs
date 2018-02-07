using System;
using System.Linq;
using Bnf.CodeItems;
using Bnf.DataTypes;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Features
{
    sealed class Result : DumpableObject
    {
        sealed class Issue : DumpableObject
        {
            readonly IssueId IssueId;
            DataTypes.Issue Cache;
            public Issue(IssueId issueId) => IssueId = issueId;

            public DataType Type(SourcePart position)
                => Cache ?? (Cache = new DataTypes.Issue(IssueId, position));
        }

        static int NextObjectId;

        public static Result Create(SourcePart position, IssueId issueId)
            => new Result(position, null, null, null, null, issueId);

        public static Result Create
        (
            SourcePart position,
            DataType dataType,
            params CodeItem[] codeItems
        )
            => new Result(position, dataType, codeItems, null, null, null);

        public static Result Create
        (
            SourcePart position,
            DataType dataType,
            Func<CodeItem[]> getCodeItems
        )
            => new Result(position, dataType, null, null, getCodeItems, null);

        public static Result Create
        (
            SourcePart position,
            Func<DataType> getDataType = null,
            Func<CodeItem[]> getCodeItems = null
        )
            => new Result(position, null, null, getDataType, getCodeItems, null);

        public static Result Empty(SourcePart position)
            => Create(position, DataType.Void);

        readonly Func<CodeItem[]> GetCodeItems;
        readonly Func<DataType> GetDataType;

        readonly SourcePart Position;

        [EnableDump]
        [EnableDumpExcept(null)]
        Issue IssueCache;

        [EnableDump]
        [EnableDumpExcept(null)]
        ResultValue ValueCache;

        Result
        (
            SourcePart position,
            DataType dataType,
            CodeItem[] codeItems,
            Func<DataType> getDataType,
            Func<CodeItem[]> getCodeItems,
            IssueId issueId)
            : base(NextObjectId++)
        {
            Position = position;
            GetDataType = getDataType;
            GetCodeItems = getCodeItems;
            Value.DataType = dataType;
            Value.CodeItems = codeItems;
            if(issueId != null)
                IssueCache = new Issue(issueId);
        }

        [DisableDump]
        public CodeItem[] CodeItems
            => Value == null
                ? new CodeItem[0]
                : GetValue(Feature.CodeItems).CodeItems;

        [DisableDump]
        public DataType DataType
            => IssueCache != null
                ? IssueCache.Type(Position)
                : GetValue(Feature.DataType).DataType;

        ResultValue Value => IssueCache != null ? null : (ValueCache ?? (ValueCache = new ResultValue()));

        [DisableDump]
        public int ByteSize
        {
            get
            {
                if(Value == null)
                    return 0;

                if(Value.CodeItems != null)
                    return Value.CodeItems.GetByteSize();

                if(Value.DataType != null)
                    return Value.DataType.ByteSize;

                NotImplementedMethod();
                return -1;
            }
        }

        [DisableDump]
        public Result Rereference => Create
        (
            Position,
            DataType.Dereference,
            CodeItems.Concat(new[] {CodeItem.CreateDereference(DataType.Dereference.ByteSize)}).ToArray());

        [DisableDump]
        public Result Dereference
        {
            get
            {
                var dataType = DataType.Dereference;
                return Create
                (
                    Position,
                    dataType,
                    () => CodeItems.Concat(new[] {CodeItem.CreateDereference(dataType.ByteSize)}).ToArray());
            }
        }

        [DisableDump]
        public Result ToVoid
        {
            get
            {
                var size = DataType.ByteSize;
                if(size == 0)
                    return this;

                return Create
                (
                    Position,
                    DataType.Void,
                    () => CodeItem.Combine(CodeItems, CodeItem.CreateDrop(size), null).ToArray()
                );
            }
        }

        public Result Combine(Result result)
        {
            NotImplementedMethod(result);
            return null;
        }

        ResultValue GetValue(Feature feature)
        {
            ReplendishValue(feature);
            return Value;
        }

        void ReplendishValue(Feature feature)
        {
            var effectiveFeature = feature - Value.CompleteFeature;
            if(effectiveFeature == Feature.None)
                return;

            if(Feature.DataType <= effectiveFeature)
                Value.DataType = GetDataType();

            if(Feature.CodeItems <= effectiveFeature)
                Value.CodeItems = GetCodeItems();
        }

        public Result Subscription(SourcePart position, Result value)
            => Create
            (
                position,
                () => DataType,
                () => CodeItem.Combine(value.CodeItems, null, CodeItems).ToArray());

        public Result Reassign(SourcePart position, Result source)
        {
            return Create
            (
                position,
                () => DataType,
                () => CodeItem.Combine
                    (
                        CodeItems,
                        null,
                        source.CodeItems,
                        CodeItem.CreateReassign(DataType.Dereference.ByteSize)
                    )
                    .ToArray()
            );
        }
    }
}