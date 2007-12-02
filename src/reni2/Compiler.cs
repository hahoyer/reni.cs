﻿using System;
using System.Collections.Generic;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using HWClassLibrary.IO;
using NUnit.Framework;
using Reni.Code;
using Reni.Context;
using Reni.FeatureTest;
using reni.Generated;
using Reni.Parser;
using Reni.Runtime;
using Base=Reni.Syntax.Base;

namespace Reni
{
    /// <summary>
    /// The compiler for language "Reni"
    /// </summary>
    public sealed class Compiler
    {
        private readonly ParserInst _parser = new ParserInst();
        private readonly string _fileName;
        private readonly Root _rootContext = Context.Base.CreateRoot();

        private Source _source;
        private Base _syntax;
        private Result _result;
        private List<Container> _functionContainers;
        private Container _mainContainer;
        private string _executedCode;
        private readonly CompilerParameters _parameters;


        /// <summary>
        /// ctor from file
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters"></param>
        public Compiler(CompilerParameters parameters, string fileName)
        {
            _fileName = fileName;
            _parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Compiler"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// created 14.07.2007 15:59 on HAHOYER-DELL by hh
        public Compiler(string fileName): this(new CompilerParameters(), fileName)
        {
        }

        /// <summary>
        /// Performs compilation
        /// </summary>
        public OutStream Exec()
        {
            if (_parameters.Trace.Source)
                Tracer.Line(Source.Dump());

            if (_parameters.Trace.Syntax)
                Tracer.FlaggedLine(Syntax.Dump());

            if (_parameters.Trace.Functions)
            {
                Materialize();
                for (int i = 0; i < RootContext.Functions.Count; i++)
                    Tracer.FlaggedLine(RootContext.Functions[i].DumpFunction());
            }

            if (_parameters.Trace.CodeTree)
            {
                Tracer.FlaggedLine("main\n" + Result.Code.Dump());
                for (int i = 0; i < Functions.Count; i++)
                    Tracer.FlaggedLine("function index="+ i + "\n"+ RootContext.Functions[i].BodyCode.Dump());
            }
            if (_parameters.Trace.CodeSequence)
            {
                Tracer.FlaggedLine("main\n" + MainContainer.Dump());
                for (int i = 0; i < FunctionContainers.Count; i++)
                    Tracer.FlaggedLine("function index=" + i + "\n" + FunctionContainers[i].Dump());
            }

            if (_parameters.Trace.ExecutedCode)
                Tracer.FlaggedLine(ExecutedCode);

            return GetOutStream();
        }

        internal FunctionList Functions { get { return RootContext.Functions; } }

        internal void Materialize()
        {
            Code.Base dummy = Result.Code;
            for (int i = 0; i < RootContext.Functions.Count; i++)
                dummy = RootContext.Functions[i].BodyCode;
            Tracer.Assert(dummy != null);
        }

        public OutStream GetOutStream()
        {
            BitsConst.OutStream = new OutStream();
            try
            {
                Assembly assembly = Generator.CreateCSharpAssembly(MainContainer, FunctionContainers, false);
                MethodInfo methodInfo = assembly.GetExportedTypes()[0].GetMethod(Generator.MainMethodName);
                methodInfo.Invoke(null, new object[0]);
            }
            catch (CompilerErrorException e)
            {
                for (int i = 0; i < e.CompilerErrorCollection.Count; i++)
                    BitsConst.OutStream.Add(e.CompilerErrorCollection[i].ToString());
            }
            return BitsConst.OutStream;
        }

        [Node, DumpData(false)]
        internal ParserInst Parser { get { return _parser; } }

        [Node, DumpData(false)]
        public string FileName { get { return _fileName; } }

        [Node, DumpData(false)]
        public Source Source
        {
            get
            {
                if (_source == null)
                    _source = new Source(File.m(_fileName));
                return _source;
            }
        }

        [Node, DumpData(false)]
        internal Base Syntax
        {
            get
            {
                if (_syntax == null)
                    _syntax = _parser.Compile(Source);
                return _syntax;
            }
        }

        [Node, DumpData(false)]
        internal Root RootContext { get { return _rootContext; } }

        [DumpData(false)]
        public string ExecutedCode
        {
            get
            {
                if (_executedCode == null)
                    _executedCode = Generator.CreateCSharpString(MainContainer, FunctionContainers, true);
                return _executedCode;
            }
        }

        [Node, DumpData(false)]
        internal Result Result
        {
            get
            {
                if (_result == null)
                    _result = Syntax.MainVisit(RootContext);

                return _result;
            }
        }

        [Node, DumpData(false)]
        internal Container MainContainer
        {
            get
            {
                if (_mainContainer == null)
                    _mainContainer = Result.Code.Serialize();
                return _mainContainer;
            }
        }

        [Node, DumpData(false)]
        internal List<Container> FunctionContainers
        {
            get
            {
                if (_functionContainers == null)
                    _functionContainers = RootContext.CompileFunctions();

                return _functionContainers;
            }
        }

        public static string FormattedNow
        {
            get
            {
                DateTime n = DateTime.Now;
                string result = "reni_";
                result += n.Year.ToString("0000");
                result += n.Month.ToString("00");
                result += n.Day.ToString("00");
                result += "_";
                result += n.Hour.ToString("00");
                result += n.Minute.ToString("00");
                result += n.Second.ToString("00");
                result += "_";
                result += n.Millisecond.ToString("000");
                return result;
            }
        }
    }

    /// <summary>
    /// Parameters for compilation
    /// </summary>
    public class CompilerParameters
    {
        public class TraceParamters
        {
            /// <summary>
            /// Shows or hides syntax tree
            /// </summary>
            [Node, DumpData(true)]
            public bool Source = false;

            /// <summary>
            /// Shows or hides syntax tree
            /// </summary>
            [Node, DumpData(true)]
            public bool Syntax = false;

            /// <summary>
            /// Shows or hides code tree
            /// </summary>
            [Node, DumpData(true)]
            public bool CodeTree = false;

            /// <summary>
            /// Shows or hides serialize code sequence
            /// </summary>
            [Node, DumpData(true)]
            public bool CodeSequence = false;

            /// <summary>
            /// Shows or hides code code to execute
            /// </summary>
            [Node, DumpData(true)]
            public bool ExecutedCode = false;

            [Node, DumpData(true)]
            public bool Functions = false;

            public void None()
            {
                Source = false;
                Syntax = false;
                CodeTree = false;
                CodeSequence = false;
                ExecutedCode = false;
                Functions = false;
            }

            public void All()
            {
                Source = true;
                Syntax = true;
                CodeTree = true;
                CodeSequence = true;
                ExecutedCode = true;
                Functions = true;
            }
          
        }
        /// <summary>
        /// Shows or hides syntax tree
        /// </summary>
        [Node, DumpData(true)]
        public TraceParamters Trace = new TraceParamters();

    }


    [TestFixture]
    public class Generated
    {
        /// <summary>
        /// Special test, will not work automatically.
        /// </summary>
        /// created 18.07.2007 01:29 on HAHOYER-DELL by hh
        [Test,Explicit, Category(CompilerTest.Rare)]
        public void Exec()
        {
            OutStream os = BitsConst.OutStream;
            BitsConst.OutStream = new OutStream();
            reni_Test.reni();
            OutStream osNew = BitsConst.OutStream;
            BitsConst.OutStream = os;
        }
    }
}