using System;
using System.Linq;
using System.Threading.Tasks;
using hw.DebugFormatter;
using OmniSharp.Extensions.LanguageServer.Server;

namespace ReniLSP
{
    static class MainContainer
    {
        public static async Task Main(string[] args)
        {
            if(args.SingleOrDefault() == "none")
            {
                "Doing nothing".Log();
                return;
            }

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