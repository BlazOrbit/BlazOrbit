using System.Runtime.CompilerServices;

namespace BlazOrbit.Core.CodeGeneration.Tests.Infrastructure;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.DontScrubGuids();
        VerifierSettings.DontScrubDateTimes();
        Verifier.DerivePathInfo((sourceFile, projectDirectory, type, method) =>
            new PathInfo(
                directory: System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sourceFile)!, "Snapshots"),
                typeName: type.Name,
                methodName: method.Name));
    }
}
