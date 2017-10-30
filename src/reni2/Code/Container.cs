using System.Linq;
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

        Container(string errorText) => Description = errorText;

        internal Container(CodeBase data, Issue[] issues, string description, FunctionId functionId = null)
            : base(_nextObjectId++)
        {
            Description = description;
            Data = data;
            Issues = issues;
            FunctionId = functionId;

            Tracer.Assert(HasCode != HasIssues);
        }

        bool HasIssues => Issues?.Any() ?? false;
        bool HasCode => Data != null;

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
            if(HasIssues)
                return "";
            var generator = new CSharpGenerator(MaxSize.SaveByteCount);
            Data.Visit(generator);
            return generator.Data.Indent(indent);
        }
    }
}