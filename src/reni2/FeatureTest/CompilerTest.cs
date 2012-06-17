// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.UnitTest;
using Reni.Runtime;

namespace Reni.FeatureTest
{
    /// <summary>
    ///     Helper class for unittests, that compile something
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CompilerTest : DependantAttribute
    {
        internal readonly CompilerParameters Parameters = new CompilerParameters();
        static Dictionary<System.Type, CompilerTest> _cache;
        bool _needToRunDependants = true;

        internal void CreateFileAndRunCompiler(string name, string text, string expectedOutput) { CreateFileAndRunCompiler(1, name, text, null, expectedOutput); }
        internal void CreateFileAndRunCompiler(string name, string text, Action<Compiler> expectedResult) { CreateFileAndRunCompiler(1, name, text, expectedResult, ""); }
        internal void CreateFileAndRunCompiler(string name, string text, Action<Compiler> expectedResult, string expectedOutput) { CreateFileAndRunCompiler(1, name, text, expectedResult, expectedOutput); }

        void CreateFileAndRunCompiler(int depth, string name, string text, Action<Compiler> expectedResult, string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = fileName.FileHandle();
            f.String = text;
            InternalRunCompiler(depth + 1, fileName, expectedResult, expectedOutput);
        }

        public static void Run(string name, string target, string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = fileName.FileHandle();
            f.String = target;
            InternalRunCompiler(CompilerParameters.CreateTraceAll(), fileName, null, expectedOutput);
        }

        void InternalRunCompiler(int depth, string fileName, Action<Compiler> expectedResult, string expectedOutput)
        {
            Tracer.FlaggedLine(depth + 1, FilePositionTag.Test, "Position of method tested");
            if(TestRunner.IsModeErrorFocus)
                Parameters.Trace.All();

            //Parameters.RunFromCode = true;
            InternalRunCompiler(Parameters, fileName, expectedResult, expectedOutput);
        }

        static void InternalRunCompiler(CompilerParameters compilerParameters, string fileName, Action<Compiler> expectedResult, string expectedOutput)
        {
            var outStream = new OutStream();
            compilerParameters.OutStream = outStream;
            var c = new Compiler(fileName, compilerParameters, "Reni");

            if(expectedResult != null)
            {
                c.Materialize();
                expectedResult(c);
            }

            c.Exec();

            if(outStream.Data == expectedOutput)
                return;

            Tracer.Line("---------------------\n" + outStream.Data + "\n---------------------");
            Tracer.ThrowAssertionFailed(
                "outStream.Data != expectedOutput",
                () => "outStream.Data:" + outStream.Data + " expected: " + expectedOutput);
        }

        void RunDependant()
        {
            RunDependants();
            Run();
        }

        [Test]
        public virtual void Run() { BaseRun(1); }

        protected void BaseRun(int depth = 0)
        {
            if(_cache == null)
                _cache = new Dictionary<System.Type, CompilerTest>();

            foreach(var tuple in TargetSet)
                CreateFileAndRunCompiler(depth + 1, GetType().PrettyName(), tuple.Item1, AssertValid, tuple.Item2);
        }


        void RunDependants()
        {
            if(!_needToRunDependants)
                return;

            _needToRunDependants = false;

            if(_cache.ContainsKey(GetType()))
                return;

            _cache.Add(GetType(), this);

            foreach(var dependsOnType in DependsOn.Where(dependsOnType => !_cache.ContainsKey(dependsOnType)))
                ((CompilerTest) Activator.CreateInstance(dependsOnType)).RunDependant();
        }

        IEnumerable<Tuple<string, string>> TargetSet
        {
            get
            {
                var result = GetStringPairAttributes<TargetSetAttribute>();
                if(Target != "")
                    result = result.Concat(new[] {new Tuple<string, string>(Target, Output)}).ToArray();

                return result;
            }
        }

        protected virtual string Output { get { return GetStringAttribute<OutputAttribute>(); } }
        protected virtual string Target { get { return GetStringAttribute<TargetAttribute>(); } }

        protected virtual IEnumerable<System.Type> DependsOn
        {
            get
            {
                return GetType()
                    .GetCustomAttributes(typeof(CompilerTest), true)
                    .Select(o => o.GetType());
            }
        }

        internal string GetStringAttribute<T>() where T : StringAttribute
        {
            var attrs = GetType().GetCustomAttributes(typeof(T), true);
            return attrs.Length == 1 ? ((T) attrs[0]).Value : "";
        }

        Tuple<string, string>[] GetStringPairAttributes<T>() where T : StringPairAttribute
        {
            return GetType()
                .GetCustomAttributes(typeof(T), true)
                .Select(x => ((StringPairAttribute) x).Value)
                .ToArray();
        }

        protected virtual void AssertValid(Compiler c) { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    sealed class IsUnderConstructionAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    abstract class StringAttribute : Attribute
    {
        internal readonly string Value;
        protected StringAttribute(string value) { Value = value; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    sealed class OutputAttribute : StringAttribute
    {
        internal OutputAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    sealed class TargetAttribute : StringAttribute
    {
        internal TargetAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    sealed class InstanceCodeAttribute : StringAttribute
    {
        public InstanceCodeAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    sealed class TargetSetAttribute : StringPairAttribute
    {
        internal TargetSetAttribute(string target, string output)
            : base(new Tuple<string, string>(target, output)) { }
    }

    abstract class StringPairAttribute : Attribute
    {
        public readonly Tuple<string, string> Value;
        protected StringPairAttribute(Tuple<string, string> value) { Value = value; }
    }
}