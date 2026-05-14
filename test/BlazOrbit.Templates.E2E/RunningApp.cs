using System.Diagnostics;

namespace BlazOrbit.Templates.E2E;

public sealed class RunningApp : IAsyncDisposable
{
    private readonly Process _process;
    private readonly List<string> _stdOut;
    private readonly List<string> _stdErr;

    public string Url { get; }

    public string Key { get; }

    public IReadOnlyList<string> StdOut => _stdOut;

    public IReadOnlyList<string> StdErr => _stdErr;

    public RunningApp(Process process, string url, string key, List<string> stdOut, List<string> stdErr)
    {
        _process = process;
        Url = url;
        Key = key;
        _stdOut = stdOut;
        _stdErr = stdErr;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                // Kill the entire process tree (dotnet + actual server)
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
            // Already exited
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _process.WaitForExitAsync(cts.Token);
        _process.Dispose();
    }
}
