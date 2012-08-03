#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.UnitTest;
using Reni.Code;
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

        internal void CreateFileAndRunCompiler(string name, string text, string expectedOutput = null, Action<Compiler> expectedResult = null) { CreateFileAndRunCompiler(1, name, new TargetSetData(text, expectedOutput), expectedResult); }

        void CreateFileAndRunCompiler(int depth, string name, TargetSetData targetSetData, Action<Compiler> expectedResult)
        {
            var fileName = name + ".reni";
            var f = fileName.FileHandle();
            f.String = targetSetData.Target;
            InternalRunCompiler(depth + 1, fileName, expectedResult, targetSetData);
        }

        void InternalRunCompiler(int depth, string fileName, Action<Compiler> expectedResult, TargetSetData targetSet)
        {
            Tracer.FlaggedLine(depth + 1, "Position of method tested", FilePositionTag.Test);
            if(TestRunner.IsModeErrorFocus)
                Parameters.Trace.All();

            //Parameters.RunFromCode = true;
            InternalRunCompiler(Parameters, fileName, expectedResult, targetSet);
        }

        void InternalRunCompiler(CompilerParameters compilerParameters, string fileName, Action<Compiler> expectedResult, TargetSetData targetSet)
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

            if(outStream.Data != targetSet.Output)
            {
                Tracer.Line("---------------------\n" + outStream.Data + "\n---------------------");
                Tracer.ThrowAssertionFailed(
                    "outStream.Data != targetSet.Output",
                    () => "outStream.Data:" + outStream.Data + " expected: " + targetSet.Output);
            }
            if(!IsExpected(c.Issues))
            {
                Tracer.Line("---------------------\n" + c.Issues + "\n---------------------");
                Tracer.ThrowAssertionFailed(
                    "!targetSet.IsExpected(c.Issues)",
                    () => "c.Issues:" + c.Issues);
            }
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
                CreateFileAndRunCompiler(depth + 1, GetType().PrettyName(), tuple, AssertValid);
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

        TargetSetData[] TargetSet
        {
            get
            {
                var result = GetType()
                    .GetAttributes<TargetSetAttribute>(true)
                    .Select(tsa => tsa.TargetSet)
                    .ToArray();

                if(Target == "")
                    return result;

                return result
                    .Concat(new[] {new TargetSetData(Target, Output)})
                    .ToArray();
            }
        }

        protected virtual string Output { get { return GetStringAttribute<OutputAttribute>(); } }
        protected virtual string Target { get { return GetStringAttribute<TargetAttribute>(); } }
        internal virtual bool IsExpected(IEnumerable<IssueBase> issues) { return !issues.Any(); }

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

        protected virtual void AssertValid(Compiler c) { }
    }
}