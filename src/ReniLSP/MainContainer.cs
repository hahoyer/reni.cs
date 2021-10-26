using System;
using System.Threading.Tasks;
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
                    .WithHandler<TokenTarget>()
            );
            await server.WaitForExit;
        }
    }
}