using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Server;

namespace ReniLSP
{
    static class MainContainer
    {
        public static async Task Main(string[] args)
        {
            var server = await LanguageServer.From(options
                => options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithHandler<Target>()
                );
            await server.WaitForExit;
        }
    }
}