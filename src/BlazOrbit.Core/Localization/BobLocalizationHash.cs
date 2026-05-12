using System.Runtime.CompilerServices;

namespace BlazOrbit.Localization;

/// <summary>
/// FNV-1a 64-bit hash used to convert source literal strings to stable lookup keys at both
/// build time (source generator) and runtime (dynamic-key slow path). The constants are the
/// canonical reference values — do not "optimize" or change them or hashes computed in
/// different builds will diverge.
/// </summary>
public static class BobLocalizationHash
{
    private const ulong FnvOffsetBasis = 0xCBF29CE484222325UL;
    private const ulong FnvPrime = 0x100000001B3UL;

    /// <summary>Computes the FNV-1a 64-bit hash of <paramref name="source"/> over its UTF-16 code units.</summary>
    /// <remarks>
    /// The hash spans each <see cref="char"/> as two bytes (low then high) so the result is
    /// stable across endianness and matches the build-time source generator implementation.
    /// Allocation-free; safe for hot paths.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Compute(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ReadOnlySpan<char> chars = source.AsSpan();
        ulong hash = FnvOffsetBasis;
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            hash ^= (byte)(c & 0xFF);
            hash *= FnvPrime;
            hash ^= (byte)((c >> 8) & 0xFF);
            hash *= FnvPrime;
        }

        return hash;
    }
}