using CdCSharp.BuildTools;
using FluentAssertions;
using System.Reflection;

namespace BlazOrbit.BuildTools.Tests.Tests.Generators;

/// <summary>
/// Snapshot tests for every <see cref="IAssetGenerator"/> in BuildTools.
/// Catches accidental drift in generated CSS/JS scaffolding.
/// </summary>
public class AssetGeneratorSnapshotTests
{
    public static TheoryData<IAssetGenerator> Generators
    {
        get
        {
            Type assetGeneratorType = typeof(IAssetGenerator);

            // IAssetGenerator lives in CdCSharp.BuildTools; the concrete generators
            // live in BlazOrbit.BuildTools.
            Assembly targetAssembly = typeof(BlazOrbit.BuildTools.Generators.CssInitializeThemesGenerator).Assembly;

            TheoryData<IAssetGenerator> data = [];
            foreach (Type type in targetAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i == assetGeneratorType))
                .OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                IAssetGenerator instance = (IAssetGenerator)Activator.CreateInstance(type)!;
                data.Add(instance);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public async Task Should_Match_Snapshot(IAssetGenerator generator)
    {
        string content = await generator.GetContent();
        content.Should().NotBeNullOrWhiteSpace(
            because: $"{generator.GetType().Name} must produce non-empty content");

        await Verify(content)
            .UseFileName($"{generator.GetType().Name}_{generator.FileName.Replace('.', '_')}");
    }
}
