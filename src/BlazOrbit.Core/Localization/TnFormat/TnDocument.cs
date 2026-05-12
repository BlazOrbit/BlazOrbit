using System.Collections.Frozen;

namespace BlazOrbit.Localization.TnFormat;

/// <summary>
/// Parsed result of a single <c>.tn</c> translations document. A document either lives in a
/// per-culture file (no header — culture inferred from the filename) or carries one-or-more
/// <c>@ culture</c> headers when packed as a multi-culture catalogue.
/// </summary>
/// <param name="Translations">
/// Outer key = culture name (BCP-47, e.g. <c>"es-ES"</c>). Inner = hash → translation. When the
/// document has no explicit <c>@ culture</c> header all entries are stored under the
/// <see cref="string.Empty"/> key — the caller (typically the source generator) assigns the
/// culture from the filename.
/// </param>
/// <param name="Diagnostics">Errors and warnings collected during parsing. Empty when the parse succeeded cleanly.</param>
public sealed record TnDocument(
    FrozenDictionary<string, FrozenDictionary<ulong, string>> Translations,
    IReadOnlyList<TnDiagnostic> Diagnostics);

/// <summary>
/// Single parse diagnostic. Severities follow the standard <c>Error</c> / <c>Warning</c> /
/// <c>Info</c> trio so generators and tooling can promote them into Roslyn diagnostics.
/// </summary>
/// <param name="Severity">Diagnostic severity.</param>
/// <param name="Message">Human-readable description of the issue.</param>
/// <param name="LineNumber">1-based line where the diagnostic was raised; <c>-1</c> when unknown.</param>
public sealed record TnDiagnostic(TnDiagnosticSeverity Severity, string Message, int LineNumber);

/// <summary>Severity of a <see cref="TnDiagnostic"/>.</summary>
public enum TnDiagnosticSeverity
{
    /// <summary>Parser produced an entry but flagged a concern (duplicate, suspicious format).</summary>
    Warning,

    /// <summary>Parser failed to produce an entry for this region of the document.</summary>
    Error,

    /// <summary>Informational; no effect on emitted entries.</summary>
    Info
}