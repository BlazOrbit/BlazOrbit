namespace BlazOrbit.Docs.Wasm.Models;

public sealed record DocSearchItem(
    string Title,
    string Route,
    string[] Breadcrumb,
    string? Keywords = null);

public sealed record DocSearchResult(DocSearchItem Item, int Score);
