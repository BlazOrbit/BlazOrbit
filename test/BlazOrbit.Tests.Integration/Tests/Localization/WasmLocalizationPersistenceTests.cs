using BlazOrbit.Localization.Wasm;
using FluentAssertions;
using Microsoft.JSInterop;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Localization", "WasmLocalizationPersistence")]
public class WasmLocalizationPersistenceTests
{
    private static WasmLocalizationPersistence CreatePersistence(
        IJSRuntime jsRuntime) => new(jsRuntime);

    [Fact]
    public async Task Should_Return_Stored_Culture_From_Module()
    {
        // Arrange
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();

        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            Arg.Is<object[]>(args =>
                args.Length == 1 &&
                args[0]!.ToString()!.Contains("LocalStorageInterop")))
            .Returns(_ => new ValueTask<IJSObjectReference>(module));

        module.InvokeAsync<string?>("get", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<string?>("es-ES"));

        WasmLocalizationPersistence persistence = CreatePersistence(jsRuntime);

        // Act
        string? culture = await persistence.GetStoredCultureAsync();

        // Assert
        culture.Should().Be("es-ES");
    }

    [Fact]
    public async Task Should_Return_Null_When_Module_Get_Returns_Null()
    {
        // Arrange
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();

        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));

        module.InvokeAsync<string?>("get", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<string?>((string?)null));

        WasmLocalizationPersistence persistence = CreatePersistence(jsRuntime);

        // Act
        string? culture = await persistence.GetStoredCultureAsync();

        // Assert
        culture.Should().BeNull();
    }

    [Fact]
    public async Task Should_Invoke_Set_On_Module()
    {
        // Arrange
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();

        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));

        WasmLocalizationPersistence persistence = CreatePersistence(jsRuntime);

        // Act
        await persistence.SetStoredCultureAsync("fr-FR");

        // Assert
        await module.Received(1).InvokeVoidAsync(
            "set",
            Arg.Is<object[]>(args =>
                args.Length == 2 &&
                args[0]!.ToString() == "BlazOrbit.Culture" &&
                args[1]!.ToString() == "fr-FR"));
    }
}
