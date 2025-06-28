namespace Reni.Runtime.Core;

public static class File 
{
    public static string Read(string name)
    {
        Dumpable.NotImplementedFunction(name);
        return default!;
    }

    public static string Write(string name, string value)
    {
        Dumpable.NotImplementedFunction(name, value);
        return default!;
    }
}
