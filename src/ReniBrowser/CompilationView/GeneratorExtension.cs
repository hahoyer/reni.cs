using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.CSharp;

namespace ReniBrowser.CompilationView
{
    static class GeneratorExtension
    {
        static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();

        public static Assembly CreateAssemblyFromFile(this string fileName)
        {
            // Build the parameters for source compilation.
            var cp = new CompilerParameters
            {
                GenerateInMemory = true,
                CompilerOptions = "/unsafe /debug",
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(null, true)
            };

            cp.ReferencedAssemblies.AddRange(GetReferencedAssemblies(fileName.FileHandle().String));
            var cr = _provider.CompileAssemblyFromFile(cp, fileName);

            if(cr.Errors.Count > 0)
                HandleErrors(cr.Errors);

            return cr.CompiledAssembly;
        }

        static string[] GetReferencedAssemblies(string text)
        {
            return text
                .Split('\n', '\r')
                .Select(FindUsing)
                .Where(l => l != null)
                .Union(new[] {Assembly.GetCallingAssembly().Location})
                .Distinct()
                .ToArray();
        }

        static string FindUsing(string line)
        {
            if(!line.StartsWith("using "))
                return null;
            var x = line.Split(' ').Skip(1).ToArray();
            string nameSpace;

            switch(x.Length)
            {
                case 1:
                    if(!x[0].EndsWith(";"))
                        return null;
                    nameSpace = x[0].Substring(0, x[0].Length - 1);
                    break;
                case 2:
                    if(x[1] != ";")
                        return null;
                    nameSpace = x[0];
                    break;
                default:
                    return null;
            }

            var matchingType = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())
                .GetReferencedTypes()
                .FirstOrDefault(t => t.Namespace == nameSpace);
            if(matchingType != null)
                return matchingType.Assembly.Location;

            Tracer.AssertionFailed("", () => line);
            return null;
        }

        static void HandleErrors(CompilerErrorCollection cr)
        {
            for(var i = 0; i < cr.Count; i++)
                Tracer.Line(cr[i].ToString());

            throw new CSharpCompilerErrorException(cr);
        }

        sealed class CSharpCompilerErrorException : Exception
        {
            [EnableDump]
            public readonly CompilerErrorCollection CompilerErrorCollection;

            public CSharpCompilerErrorException(CompilerErrorCollection cr)
            {
                CompilerErrorCollection = cr;
            }

            public override string Message
            {
                get
                {
                    return CompilerErrorCollection
                        .Cast<CompilerError>()
                        .Stringify("\n\n");
                }
            }
        }
    }
}