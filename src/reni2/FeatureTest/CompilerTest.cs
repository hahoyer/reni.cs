using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;
using HWClassLibrary.UnitTest;
using JetBrains.Annotations;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Helper class for unittests, that compile something
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CompilerTest : DependantAttribute
    {
        [UsedImplicitly]
        public const string Damaged = "Damaged";

        [UsedImplicitly]
        public const string Rare = "Rare";

        [UsedImplicitly]
        public const string UnderConstruction = "Under Construction";

        [UsedImplicitly]
        public const string UnderConstructionNoAutoTrace = "Under Construction (No auto trace)";

        [UsedImplicitly]
        public const string Worked = "Worked";

        internal readonly CompilerParameters Parameters = new CompilerParameters();
        private static Dictionary<System.Type, CompilerTest> _cache;
        private bool _needToRunDependants = true;

        protected void CreateFileAndRunCompiler(string name, string text, string expectedOutput)
        {
            CreateFileAndRunCompiler(1, name, text, null, expectedOutput);
        }
        protected void CreateFileAndRunCompiler(string name, string text, Action<Compiler> expectedResult) { CreateFileAndRunCompiler(1, name, text, expectedResult, ""); }
        protected void CreateFileAndRunCompiler(string name, string text, Action<Compiler> expectedResult, string expectedOutput) { CreateFileAndRunCompiler(1, name, text, expectedResult, expectedOutput); }

        private void CreateFileAndRunCompiler(int depth, string name, string text, Action<Compiler> expectedResult, string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = File.m(fileName);
            f.String = text;
            InternalRunCompiler(depth + 1, fileName, expectedResult, expectedOutput);
        }

        public static void Run(string name, string target, string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = File.m(fileName);
            f.String = target;
            InternalRunCompiler(CompilerParameters.CreateTraceAll(), fileName, null, expectedOutput);
        }

        private void InternalRunCompiler(int depth, string fileName, Action<Compiler> expectedResult, string expectedOutput)
        {
            Tracer.FlaggedLine(depth + 1, "Position of method tested");
            if (TestRunner.IsModeErrorFocus || IsCallerUnderConstruction(1))
                Parameters.Trace.All();

            Parameters.RunFromCode = true;
            InternalRunCompiler(Parameters, fileName, expectedResult, expectedOutput);
        }

        private static void InternalRunCompiler(CompilerParameters compilerParameters, string fileName, Action<Compiler> expectedResult, string expectedOutput)
        {
            var c = new Compiler(compilerParameters, fileName);

            if(expectedResult != null)
            {
                c.Materialize();
                expectedResult(c);
            }

            var os = c.Exec();
            if(os != null && os.Data != expectedOutput)
            {
                os.Exec();
                Tracer.ThrowAssertionFailed(
                    "os.Data != expectedOutput",
                    () => "os.Data:" + os.Data + " expected: " + expectedOutput);
            }
        }

        private static bool IsCallerUnderConstruction(int depth)
        {
            for(var i = 0; i < 100; i++)
            {
                var x = new StackTrace(true).GetFrame(depth + i).GetMethod();
                if(x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                    return x
                        .GetCustomAttributes(typeof(CategoryAttribute), true)
                        .Any(t => ((CategoryAttribute) t).Name == UnderConstruction);
            }
            return false;
        }

        private void RunDependant()
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
                CreateFileAndRunCompiler(depth + 1, GetType().Name, tuple.Item1, AssertValid, tuple.Item2);
        }


        private void RunDependants()
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

        private IEnumerable<Tuple<string, string>> TargetSet
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
        public virtual string Target { get { return GetStringAttribute<TargetAttribute>(); } }

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

        private Tuple<string, string>[] GetStringPairAttributes<T>() where T : StringPairAttribute
        {
            return GetType()
                .GetCustomAttributes(typeof(T), true)
                .Select(x => ((StringPairAttribute) x).Value)
                .ToArray();
        }

        protected virtual void AssertValid(Compiler c) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal abstract class StringAttribute : Attribute
    {
        internal readonly string Value;
        protected StringAttribute(string value) { Value = value; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class OutputAttribute : StringAttribute
    {
        internal OutputAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class TargetAttribute : StringAttribute
    {
        internal TargetAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class InstanceCodeAttribute : StringAttribute
    {
        public InstanceCodeAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class TargetSetAttribute : StringPairAttribute
    {
        internal TargetSetAttribute(string target, string output)
            : base(new Tuple<string, string>(target, output)) { }
    }

    internal abstract class StringPairAttribute : Attribute
    {
        public readonly Tuple<string, string> Value;
        protected StringPairAttribute(Tuple<string, string> value) { Value = value; }
    }
}