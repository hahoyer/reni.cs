using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using Reni;
using Reni.FeatureTest.Integer;

namespace ReniTest
{
    public static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //InspectCompiler();
            RunTests();
        }

        private static void RunTests()
        {
            var attributeType = typeof(TestFixtureAttribute);
            var x = GetAssemblies()
                .SelectMany(assembly=> assembly.GetTypes())
                .Where(t => t.GetCustomAttributes(attributeType,true).Length > 0)
                .ToArray();
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var x = AppDomain.CurrentDomain.GetAssemblies();
            for (; ; )
            {
                var xCount = x.Length;
                var z = x.SelectMany(xx => xx.GetReferencedAssemblies()).Select(AssemblyLoad);
                x = x.Union(z).Distinct().ToArray();
                if (xCount == x.Length)
                    return x;
            }
        }

        private static Assembly AssemblyLoad(AssemblyName yy)
        {
            try
            {
                return AppDomain.CurrentDomain.Load(yy);
            }
            catch(Exception e)
            {
                return Assembly.GetExecutingAssembly();
            }
        }

        private static void InspectCompiler()
        {
            Application.Run
                (
                    new TreeForm
                        {
                            Target = CreateCompiler(new IntegerPlusNumber().Target)
                        }
                );
        }

        private static Compiler CreateCompiler(string text)
        {
            const string fileName = "temptest.reni";
            var f = File.m(fileName);
            f.String = text;
            var compiler = new Compiler(fileName);
            //Profiler.Measure(()=>compiler.Exec());
            //Tracer.FlaggedLine(Profiler.Format(10,0.0));
            return compiler;
        }
    }

}