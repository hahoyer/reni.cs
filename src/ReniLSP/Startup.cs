using System.Diagnostics;

namespace ReniLSP;

[PublicAPI]
public class Startup
{
    public async Task<object> Invoke(object input)
    {
        Debugger.Launch();
        await MainContainer.RunServer();
        return Task.CompletedTask;
    }
}