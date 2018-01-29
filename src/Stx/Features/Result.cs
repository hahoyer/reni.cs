using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Stx.CodeItems;
using Stx.DataTypes;

namespace Stx.Features
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

        readonly Func<CodeItem[]> GetCodeItems;
        readonly Func<DataType> GetDataType;

        readonly SourcePart Position;

        [EnableDump]
        [EnableDumpExcept(null)]
        Issue IssueCache;

        [EnableDump]
        [EnableDumpExcept(null)]
        ResultValue ValueCache;

        public Result
        (
            SourcePart position,
            DataType dataType = null,
            CodeItem[] codeItems = null,
            Func<DataType> getDataType = null,
            Func<CodeItem[]> getCodeItems = null)
            : base(NextObjectId++)
        {
            Tracer.Assert(dataType == null != (getDataType == null));
            Tracer.Assert(codeItems == null != (getCodeItems == null));

            Position = position;
            GetDataType = getDataType;
            GetCodeItems = getCodeItems;
            Value.DataType = dataType;
            Value.CodeItems = codeItems;
        }

        [DisableDump]
        public IssueId IssueId
        {
            set
            {
                if(IssueCache != null)
                    NotImplementedMethod(value);
                IssueCache = new Issue(value);
            }
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

        public Result Rereference => new Result
        (
            Position,
            DataType.Dereference,
            CodeItems.Concat(new[] {CodeItem.CreateDereference(DataType.Dereference.ByteSize)}).ToArray());

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


        public Result Subscription(Result value)
        {
            NotImplementedMethod(value);
            return null;
        }
    }
}