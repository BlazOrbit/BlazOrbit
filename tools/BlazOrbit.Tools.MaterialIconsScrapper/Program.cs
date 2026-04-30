using BlazOrbit.Tools.MaterialIconsScrapper;

//await IconsClassGenerator.BuildIconsFile();

// ── Generate BOBIconKeys companions from existing BOBIcons files ──
// CWD when run via `dotnet run` is the project folder.
string manualIconsDir = Path.GetFullPath(
    Path.Combine("..", "..", "src", "BlazOrbit.Core", "Media", "Icons"));

// Generated Material Icons (BOBIcons.cs → BOBIconKeys.cs)
string[] generatedSources = Directory.GetFiles(manualIconsDir, "BOBIcons*.cs");
// Manual partials (UI.cs → BOBIconKeys.UI.cs, Brands.cs → BOBIconKeys.Brands.cs, etc.)
string[] manualPartials = ["UI.cs", "Brands.cs", "FileFormats.cs"];

Console.WriteLine($"Generating BOBIconKeys from {generatedSources.Length} generated source(s)...");
foreach (string source in generatedSources)
{
    IconKeysClassGenerator.GenerateFromIconsFile(source, manualIconsDir);
}

Console.WriteLine($"Generating BOBIconKeys from {manualPartials.Length} manual partial(s)...");
foreach (string fileName in manualPartials)
{
    string source = Path.Combine(manualIconsDir, fileName);
    if (File.Exists(source))
    {
        IconKeysClassGenerator.GenerateFromIconsFile(source, manualIconsDir);
    }
    else
    {
        Console.WriteLine($"  Not found: {source}");
    }
}
