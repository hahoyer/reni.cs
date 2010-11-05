using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Runtime;

namespace Reni
{
    [Serializable]
    public sealed class Compiler : ReniObject
    {
        private readonly string _fileName;
        private readonly CompilerParameters _parameters;
        private readonly ParserInst _parser;
        private readonly Root _rootContext = ContextBase.CreateRoot();

        private readonly SimpleCache<Source> _source;
        private readonly SimpleCache<IParsedSyntax> _syntax;
        private readonly SimpleCache<CodeBase> _code;
        private readonly SimpleCache<Container> _mainContainer;
        private readonly SimpleCache<List<Container>> _functionContainers;
        private readonly SimpleCache<string> _executedCode;

        /// <summary>
        /// ctor from file
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters"></param>
        public Compiler(CompilerParameters parameters, string fileName)
        {
            _fileName = fileName;
            _parameters = parameters;
            _parser = new ParserInst(new Scanner(), MainTokenFactory.Instance);
            _source = new SimpleCache<Source>(() => new Source(File.m(FileName)));
            _syntax = new SimpleCache<IParsedSyntax>(() => _parser.Compile(Source));
            _code = new SimpleCache<CodeBase> (()=>Struct.Container.Create(Syntax).Result(_rootContext, Category.Code).Code);
            _mainContainer = new SimpleCache<Container>(() => new Container(Code));
            _functionContainers = new SimpleCache<List<Container>>(()=>_rootContext.CompileFunctions());
            _executedCode = new SimpleCache<string>(() => Generator.CreateCSharpString(MainContainer, FunctionContainers, false));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Compiler"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// created 14.07.2007 15:59 on HAHOYER-DELL by hh
        public Compiler(string fileName)
            : this(new CompilerParameters(), fileName) { }

        internal FunctionList Functions { get { return _rootContext.Functions; } }

        [IsDumpEnabled(false)]
        public string FileName { get { return _fileName; } }
        [IsDumpEnabled(false)]
        public Source Source { get { return _source.Value; } }
        [Node, IsDumpEnabled(false)]
        internal IParsedSyntax Syntax { get { return _syntax.Value; } }
        [IsDumpEnabled(false)]
        public string ExecutedCode { get { return _executedCode.Value; } }

        [Node, IsDumpEnabled(false)]
        private CodeBase Code { get { return _code.Value; } }
        [IsDumpEnabled(false)]
        private Container MainContainer { get { return _mainContainer.Value; } }
        [Node, IsDumpEnabled(false)]
        private List<Container> FunctionContainers { get { return _functionContainers.Value; } }

        public static string FormattedNow
        {
            get
            {
                var n = DateTime.Now;
                var result = "Date";
                result += n.Year.ToString("0000");
                result += n.Month.ToString("00");
                result += n.Day.ToString("00");
                result += "Time";
                result += n.Hour.ToString("00");
                result += n.Minute.ToString("00");
                result += n.Second.ToString("00");
                result += n.Millisecond.ToString("000");
                return result;
            }
        }

        /// <summary>
        /// Performs compilation
        /// </summary>
        public OutStream Exec()
        {
            if(_parameters.Trace.Source)
                Tracer.Line(Source.Dump());

            if(_parameters.Trace.Syntax)
                Tracer.FlaggedLine(Syntax.Dump());

            if(_parameters.ParseOnly)
                return null;

            if(_parameters.Trace.Functions)
            {
                Materialize();
                for(var i = 0; i < _rootContext.Functions.Count; i++)
                    Tracer.FlaggedLine(_rootContext.Functions[i].DumpFunction());
            }

            if(_parameters.Trace.CodeTree)
            {
                Tracer.FlaggedLine("main\n" + Code.Dump());
                for(var i = 0; i < Functions.Count; i++)
                    Tracer.FlaggedLine("function index=" + i + "\n" + _rootContext.Functions[i].BodyCode.Dump());
            }
            if(_parameters.Trace.CodeSequence)
            {
                Tracer.FlaggedLine("main\n" + MainContainer.Dump());
                for(var i = 0; i < FunctionContainers.Count; i++)
                    Tracer.FlaggedLine("function index=" + i + "\n" + FunctionContainers[i].Dump());
            }
            if(_parameters.Trace.ExecutedCode)
                Tracer.FlaggedLine(ExecutedCode);

            return GetOutStream();
        }

        internal void Materialize()
        {
            if (_parameters.ParseOnly)
                return;
            _code.Ensure();
            for (var i = 0; i < _rootContext.Functions.Count; i++)
                _rootContext.Functions[i].EnsureBodyCode();
        }

        public OutStream GetOutStream()
        {
            BitsConst.OutStream = new OutStream();
            try
            {
                var assembly = Generator.CreateCSharpAssembly(MainContainer, FunctionContainers, false);
                var methodInfo = assembly.GetExportedTypes()[0].GetMethod(Generator.MainFunctionName);
                methodInfo.Invoke(null, new object[0]);
            }
            catch(CompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    BitsConst.OutStream.Add(e.CompilerErrorCollection[i].ToString());
            }
            return BitsConst.OutStream;
        }
    }
}