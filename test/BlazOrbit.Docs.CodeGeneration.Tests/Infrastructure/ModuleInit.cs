using System.Runtime.CompilerServices;

namespace BlazOrbit.Docs.CodeGeneration.Tests.Infrastructure;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.DontScrubGuids();
        VerifierSettings.DontScrubDateTimes();
        DerivePathInfo((sourceFile, projectDirectory, type, method) =>
            new PathInfo(
                Path.Combine(Path.GetDirectoryName(sourceFile)!, "Snapshots"),
                type.Name,
                method.Name));
    }
}