using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using hw.DebugFormatter;
using Microsoft;
using StreamJsonRpc;

namespace ReniLSP
{
    static class MainContainer
    {
        public static async Task<int> Main(string[] args)
        {
            var converter = JsonRpc.Attach(Console.OpenStandardOutput(), Console.OpenStandardInput(), new Target());
            await converter.Completion;
            return 0;
        }
    }
}