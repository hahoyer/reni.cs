using System;
using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Helper class for unittests, that compile something
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CompilerTest : Attribute
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

        internal CompilerParameters Parameters;                    
        static private Dictionary<System.Type, CompilerTest> _cache;
        private bool _needToRunDependants = true;

        [SetUp]
        public void Start() { Parameters = new CompilerParameters(); }

        protected void CreateFileAndRunCompiler(string name, string text, string expectedOutput){CreateFileAndRunCompiler(1, name, text, null, expectedOutput);}
        protected void CreateFileAndRunCompiler(string name, string text, Action<Compiler> expectedResult) { CreateFileAndRunCompiler(1, name, text, expectedResult, ""); }
        protected void CreateFileAndRunCompiler(string name, string text, Action<Compiler> expectedResult, string expectedOutput) { CreateFileAndRunCompiler(1, name, text, expectedResult, expectedOutput); }

        private void CreateFileAndRunCompiler(int depth, string name, string text, Action<Compiler> expectedResult, string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = File.m(fileName);
            f.String = text;
            InternalRunCompiler(depth + 1, fileName, expectedResult, expectedOutput);
        }

        private void InternalRunCompiler(int depth, string fileName, Action<Compiler> expectedResult, string expectedOutput)
        {
            Tracer.FlaggedLine(depth + 1, "Position of method tested");
            if(IsCallerUnderConstruction(1))
                Parameters.Trace.All();

            var c = new Compiler(Parameters, fileName);

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
                    "os.Data:" + os.Data + " expected: " + expectedOutput);
            }
        }

        private static bool IsCallerUnderConstruction(int depth)
        {
            for(var i = 0; i < 100; i++)
            {
                var x = new StackTrace(true).GetFrame(depth + i).GetMethod();
                if(x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                {
                    var xx = x.GetCustomAttributes(typeof(CategoryAttribute), true);
                    for(var ii = 0; ii < xx.Length; ii++)
                        if(((CategoryAttribute) xx[ii]).Name == UnderConstruction)
                            return true;
                    return false;
                }
            }
            return false;
        }

        private void RunDependant()
        {
            RunDependants();
            Start();
            Run();
        }

        public abstract void Run();

        protected void BaseRun()
        {
            if(_cache == null)
                _cache = new Dictionary<System.Type, CompilerTest>();
            
            RunDependants();

            CreateFileAndRunCompiler(1, GetType().Name, Target, AssertValid, Output);
        }

        private void RunDependants()
        {
            if (!_needToRunDependants)
                return;
            
            _needToRunDependants = false;

            if (_cache.ContainsKey(GetType()))
                return;

            _cache.Add(GetType(), this);

            foreach(var dependsOnType in DependsOn)
                if(!_cache.ContainsKey(dependsOnType))
                    ((CompilerTest) Activator.CreateInstance(dependsOnType)).RunDependant();
        }

        public virtual string Output { get { return GetStringAttribute<OutputAttribute>(); } }
        public virtual string Target { get { return GetStringAttribute<TargetAttribute>(); } }
        public virtual System.Type[] DependsOn { get { return ToTypes(GetType().GetCustomAttributes(typeof(CompilerTest), true)); } }

        private static System.Type[] ToTypes(object[] objects)
        {
            var result = new List<System.Type>();
            foreach(var o in objects)
                result.Add(o.GetType());
            return result.ToArray();
        }

        internal string GetStringAttribute<T>() where T : StringAttribute
        {
            var attrs = GetType().GetCustomAttributes(typeof(T), true);
            if (attrs.Length == 1)
                return ((T)attrs[0]).Value;
            return "";
        }

        public virtual void AssertValid(Compiler c) { }
    }

    internal abstract class StringAttribute : Attribute
    {
        internal string Value;
        protected StringAttribute(string value) { Value = value; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class OutputAttribute : StringAttribute
    {
        internal OutputAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class TargetAttribute : StringAttribute
    {
        internal TargetAttribute(string value)
            : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class InstanceCodeAttribute : StringAttribute
    {
        public InstanceCodeAttribute(string value)
            : base(value) { }
    }

}