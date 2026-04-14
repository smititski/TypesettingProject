namespace UI_Application.Constants;

/// <summary>
/// קבועי מערכת העימוד הדו-טורית
/// Traditional Scholarly Page Layout — 3-Commentary Edition
/// </summary>
public static class LayoutConstants
{
    // ============================================
    // מידות עמוד A4 ב-96 DPI (WPF default)
    // ============================================
    public const double PAGE_WIDTH_PX = 794;        // A4 at 96dpi
    public const double PAGE_HEIGHT_PX = 1123;    // A4 at 96dpi
    
    // ============================================
    // שוליים (בפיקסלים)
    // ============================================
    public const double MARGIN_TOP = 72;
    public const double MARGIN_BOTTOM = 80;
    public const double MARGIN_OUTER = 60;
    public const double MARGIN_INNER = 50;
    public const double COLUMN_GAP = 24;
    
    // ============================================
    // חישוב רוחב טור אוטומטי
    // ============================================
    public static readonly double COLUMN_WIDTH = (PAGE_WIDTH_PX - MARGIN_OUTER - MARGIN_INNER - COLUMN_GAP) / 2;
    
    // ============================================
    // גופנים
    // ============================================
    public const string FONT_MAIN = "Frank Ruehl CLM";
    public const string FONT_FALLBACK = "David";
    
    // ============================================
    // גדלי גופן בנקודות (pt)
    // ============================================
    public const double FONT_SIZE_MAIN = 13.0;           // טקסט ראשי
    public const double FONT_SIZE_FOOTNOTE_A = 10.5;     // הערות מערכת 1
    public const double FONT_SIZE_FOOTNOTE_B = 9.5;      // הערות מערכת 2
    public const double FONT_SIZE_FOOTNOTE_C = 9.0;      // הערות מערכת 3
    public const double FONT_SIZE_RUNNING_HEADER = 9.0;  // כותרת רצה
    public const double FONT_SIZE_PAGE_NUMBER = 9.0;     // מספר עמוד
    
    // ============================================
    // גובה שורה (מכפלה)
    // ============================================
    public const double LINE_HEIGHT_MAIN = 1.4;
    public const double LINE_HEIGHT_FOOTNOTE = 1.3;
    
    // ============================================
    // עובי קווים ומפרידים
    // ============================================
    public const double SEPARATOR_LINE_THICKNESS = 0.5;
    public const double HEADER_LINE_THICKNESS = 0.5;
    
    // ============================================
    // מרווחים וגובה כותרות
    // ============================================
    public const double RUNNING_HEADER_HEIGHT = 24;
    public const double RUNNING_HEADER_OFFSET = 20;      // מרחק מראש העמוד
    public const double PAGE_NUMBER_OFFSET = 18;       // מרחק מראש העמוד
    public const double FOOTNOTE_SECTION_TITLE_HEIGHT = 20;
    public const double FOOTNOTE_AREA_MIN_LINES = 3;   // מינימום שורות להערות
    
    // ============================================
    // צבעים
    // ============================================
    public const string COLOR_RUNNING_HEADER = "#666666";
    public const string COLOR_SEPARATOR_LINE = "#999999";
    public const string COLOR_TEXT = "#000000";
    
    // ============================================
    // סימני מרקרים בטקסט
    // ============================================
    public const string MARKER_FOOTNOTE_A = "$$";  // התחלת שורת הערה A
    public const string MARKER_FOOTNOTE_B = "%%";  // התחלת שורת הערה B
    public const string MARKER_FOOTNOTE_C = "&&";  // התחלת שורת הערה C
    public const string REF_PREFIX_A = "$";        // מרקר בהערה A
    public const string REF_PREFIX_B = "%";        // מרקר בהערה B
    public const string REF_PREFIX_C = "&";        // מרקר בהערה C
    
    // ============================================
    // זיהוי כותרות ופרשות
    // ============================================
    public const string MARKER_BOOK_NAME = "===";    // ===בראשית
    public const string MARKER_PARASHA = "---";     // ---בראשית
    
    // ============================================
    // יוניקוד ניקוד וטעמים (טווחים להסרה במדידה)
    // ============================================
    public const int NIKUD_START = 0x05B0;
    public const int NIKUD_END = 0x05C7;
}
