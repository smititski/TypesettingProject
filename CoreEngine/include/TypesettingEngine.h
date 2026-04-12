#ifndef TYPESETTINGENGINE_H
#define TYPESETTINGENGINE_H

#ifdef _WIN32
    #ifdef TYPSETTINGCORE_EXPORTS
        #define TYPESETTING_API __declspec(dllexport)
    #else
        #define TYPESETTING_API __declspec(dllimport)
    #endif
#else
    #define TYPESETTING_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @struct GlyphInfo
 * @brief Structure representing a single glyph with its position.
 * 
 * Memory alignment: Packed to 12 bytes (4 + 4 + 4)
 * Must match exactly with C# struct layout.
 */
typedef struct {
    int id;     // Glyph ID
    float x;    // X coordinate
    float y;    // Y coordinate
} GlyphInfo;

/**
 * @brief Layout text and generate glyph information.
 * 
 * @param text Input text string (UTF-8 encoded)
 * @param outGlyphs Output array to receive glyph information (pre-allocated by caller)
 * @param count Input: maximum capacity of outGlyphs array. Output: actual number of glyphs generated.
 * 
 * @note The caller must allocate sufficient memory for outGlyphs before calling.
 * @note This function uses Cdecl calling convention for P/Invoke compatibility.
 */
TYPESETTING_API void LayoutText(const wchar_t* text, GlyphInfo* outGlyphs, int* count);

#ifdef __cplusplus
}
#endif

#endif // TYPESETTINGENGINE_H