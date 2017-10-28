using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Struct;
using Reni.Validation;

namespace Reni.Code
{
    sealed class Container : DumpableObject
    {
        static int _nextObjectId;

        [Node]
        internal readonly FunctionId FunctionId;

        [Node]
        internal readonly Issue[] Issues;

        internal Container(CodeBase data, string description, FunctionId functionId = null)
            : base(_nextObjectId++)
        {
            Description = description;
            FunctionId = functionId;
            Data = data;
            StopByObjectIds(-10);
        }

        Container(string errorText) => Description = errorText;

        internal Container(Issue[] issues, string description)
            : base(_nextObjectId++)
        {
            Description = description;
            Issues = issues;
        }

        [Node]
        [EnableDump]
        internal CodeBase Data { get; }

        [Node]
        [EnableDump]
        internal string Description { get; }

        [Node]
        [DisableDump]
        internal Size MaxSize => Data?.TemporarySize ?? Size.Zero;

        [Node]
        [DisableDump]
        public static Container UnexpectedVisitOfPending { get; }
            = new Container(errorText: "UnexpectedVisitOfPending");

        public string GetCSharpStatements(int indent)
        {
            if(Issues != null)
                return "";
            var generator = new CSharpGenerator(MaxSize.SaveByteCount);
            Data.Visit(generator);
            return generator.Data.Indent(indent);
        }
    }
}