using System.Runtime.InteropServices;

namespace UI_Application;

/// <summary>
/// Native methods for P/Invoke to the C++ TypesettingCore.dll.
/// 
/// IMPORTANT: Uses Unicode (UTF-16) encoding for proper Hebrew character support.
/// 
/// MEMORY SAFETY NOTES:
/// - The GlyphInfo struct uses LayoutKind.Sequential with explicit Size=12
/// - String marshaling uses CharSet.Unicode (UTF-16 wide strings)
/// - The marshaller automatically allocates and frees the native string copy
/// - No manual memory management required for string parameter
/// - CallingConvention.Cdecl matches C++ extern "C" export
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 12, Pack = 4)]
public struct GlyphInfo
{
    public int Id;      // Glyph ID (Unicode code point)
    public float X;     // X coordinate (4 bytes)
    public float Y;     // Y coordinate (4 bytes)
}

/// <summary>
/// Static class containing P/Invoke declarations for the typesetting engine.
/// 
/// All P/Invoke signatures are designed for memory safety:
/// - Strings are marshaled as Unicode (UTF-16) for full Hebrew support
/// - Arrays use [Out] attribute to indicate data flows from native to managed
/// - Struct layout is explicitly controlled via attributes
/// </summary>
public static class NativeMethods
{
    private const string DllName = "TypesettingCore.dll";

    /// <summary>
    /// Layout text and generate glyph information.
    /// </summary>
    /// <param name="text">Input text string (Unicode/UTF-16, marshaled automatically)</param>
    /// <param name="outGlyphs">Output array to receive glyph information (pre-allocated by caller)</param>
    /// <param name="count">Input: maximum capacity. Output: actual glyph count.</param>
    /// <remarks>
    /// MEMORY SAFETY:
    /// - Uses Cdecl calling convention to match C++ extern "C" export
    /// - String is marshaled as Unicode (LPWStr) for Hebrew character support
    /// - outGlyphs array must be pre-allocated with sufficient capacity
    /// - count parameter is passed by reference (ref int)
    /// - No native memory leaks - all marshaling is managed by the CLR
    /// </remarks>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern void LayoutText(
        [MarshalAs(UnmanagedType.LPWStr)] string text, 
        [Out] GlyphInfo[] outGlyphs, 
        ref int count);
}