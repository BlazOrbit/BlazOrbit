using CdCSharp.BuildTools;

string projectPath = args.Length > 0 ? args[0] : ".";

await BuildToolsManager.Build(projectPath);