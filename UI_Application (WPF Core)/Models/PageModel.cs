using System.Collections.Generic;

namespace UI_Application.Models
{
    /// <summary>
    /// מודל נתונים לעמוד עימוד דו-טורי עם תמיכה ב-3 מערכות הערות
    /// Traditional Scholarly Page Layout — 3-Commentary Edition
    /// </summary>
    public class PageModel
    {
        // ═══════════════════════════════════════════════════════════
        // שדות בסיסיים
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// מספר העמוד — מוצג בפינה השמאלית-עליונה (או ימנית-עליונה בדפי versa)
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// האם זה עמוד versa (דף שני בפתיחה) — משפיע על מיקום מספר העמוד
        /// </summary>
        public bool IsVerso { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // כותרת רצה (Running Header)
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// חלק ימין של הכותרת הרצה — מידע על פרק/פסוק
        /// </summary>
        public string RunningHeaderRight { get; set; } = string.Empty;
        
        /// <summary>
        /// חלק מרכזי של הכותרת הרצה — שם הפרשה/ספר
        /// </summary>
        public string RunningHeaderCenter { get; set; } = string.Empty;
        
        /// <summary>
        /// חלק שמאל של הכותרת הרצה — מידע נוסף
        /// </summary>
        public string RunningHeaderLeft { get; set; } = string.Empty;
        
        /// <summary>
        /// שם הספר (לתאימות לאחור)
        /// </summary>
        public string BookName { get; set; } = string.Empty;
        
        // ═══════════════════════════════════════════════════════════
        // טקסט ראשי — מבנה דו-טורי
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// טור ימין — שורות טקסט ראשי (עמודה 2 ב-RTL)
        /// </summary>
        public List<string> MainContentColumnRight { get; set; } = new List<string>();
        
        /// <summary>
        /// טור שמאל — שורות טקסט ראשי (עמודה 1 ב-RTL)
        /// </summary>
        public List<string> MainContentColumnLeft { get; set; } = new List<string>();
        
        /// <summary>
        /// כל השורות ביחד (לפני החלוקה לטורים) — לשימוש פנימי
        /// </summary>
        public List<string> AllMainLines { get; set; } = new List<string>();
        
        // ═══════════════════════════════════════════════════════════
        /// מערכת הערות 1 (פירוש ראשון) — Hebrew letters (א, ב, ג)
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// כותרת מערכת הערות 1
        /// </summary>
        public string FootnoteATitle { get; set; } = "פירוש ראשון";
        
        /// <summary>
        /// הערות טור ימין — מערכת 1
        /// </summary>
        public List<string> FootnoteAColumnRight { get; set; } = new List<string>();
        
        /// <summary>
        /// הערות טור שמאל — מערכת 1
        /// </summary>
        public List<string> FootnoteAColumnLeft { get; set; } = new List<string>();
        
        /// <summary>
        /// כל ההערות המקוריות (לפני החלוקה לטורים)
        /// </summary>
        public List<string> AllFootnotesA { get; set; } = new List<string>();
        
        // ═══════════════════════════════════════════════════════════
        /// מערכת הערות 2 (פירוש שני) — Numerals (1, 2, 3)
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// כותרת מערכת הערות 2
        /// </summary>
        public string FootnoteBTitle { get; set; } = "פירוש שני";
        
        /// <summary>
        /// הערות טור ימין — מערכת 2
        /// </summary>
        public List<string> FootnoteBColumnRight { get; set; } = new List<string>();
        
        /// <summary>
        /// הערות טור שמאל — מערכת 2
        /// </summary>
        public List<string> FootnoteBColumnLeft { get; set; } = new List<string>();
        
        /// <summary>
        /// כל ההערות המקוריות (לפני החלוקה לטורים)
        /// </summary>
        public List<string> AllFootnotesB { get; set; } = new List<string>();
        
        // ═══════════════════════════════════════════════════════════
        /// מערכת הערות 3 (מדרש/הערות נוספות) — Symbols (*, †, ‡)
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// כותרת מערכת הערות 3
        /// </summary>
        public string FootnoteCTitle { get; set; } = "מדרש";
        
        /// <summary>
        /// הערות טור ימין — מערכת 3
        /// </summary>
        public List<string> FootnoteCColumnRight { get; set; } = new List<string>();
        
        /// <summary>
        /// הערות טור שמאל — מערכת 3
        /// </summary>
        public List<string> FootnoteCColumnLeft { get; set; } = new List<string>();
        
        /// <summary>
        /// כל ההערות המקוריות (לפני החלוקה לטורים)
        /// </summary>
        public List<string> AllFootnotesC { get; set; } = new List<string>();
        
        // ═══════════════════════════════════════════════════════════
        // מיפויי מיקומים — לכותרת רצה ולחישובים
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// מיפוי: שורה → מיקום פסוק (לכותרת רצה)
        /// </summary>
        public Dictionary<int, VerseLocation> LineToVerseLocation { get; set; } = new Dictionary<int, VerseLocation>();
        
        /// <summary>
        /// מיפוי: שורה → אינדקסי הערות מערכת 1 השייכות אליה
        /// </summary>
        public Dictionary<int, List<int>> LineToFootnoteA { get; set; } = new Dictionary<int, List<int>>();
        
        /// <summary>
        /// מיפוי: שורה → אינדקסי הערות מערכת 2 השייכות אליה
        /// </summary>
        public Dictionary<int, List<int>> LineToFootnoteB { get; set; } = new Dictionary<int, List<int>>();
        
        /// <summary>
        /// מיפוי: שורה → אינדקסי הערות מערכת 3 השייכות אליה
        /// </summary>
        public Dictionary<int, List<int>> LineToFootnoteC { get; set; } = new Dictionary<int, List<int>>();
        
        // ═══════════════════════════════════════════════════════════
        // מידות העמוד
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// רוחב העמוד בפיקסלים
        /// </summary>
        public double PageWidth { get; set; } = 700;
        
        /// <summary>
        /// גובה העמוד בפיקסלים
        /// </summary>
        public double PageHeight { get; set; } = 950;
        
        /// <summary>
        /// רוחב טור בודד
        /// </summary>
        public double ColumnWidth => (PageWidth - MarginOuter * 2 - ColumnGap) / 2;
        
        /// <summary>
        /// שוליים חיצוניים (ימין/שמאל)
        /// </summary>
        public double MarginOuter { get; set; } = 40;
        
        /// <summary>
        /// שוליים פנימיים (מרווח בין הטורים)
        /// </summary>
        public double MarginInner { get; set; } = 15;
        
        /// <summary>
        /// רווח בין הטורים
        /// </summary>
        public double ColumnGap => MarginInner * 2;
        
        /// <summary>
        /// גובה כותרת רצה
        /// </summary>
        public double HeaderHeight { get; set; } = 50;
        
        /// <summary>
        /// גובה שורת טקסט ראשי
        /// </summary>
        public double MainLineHeight { get; set; } = 28;
        
        /// <summary>
        /// גובה שורת הערה
        /// </summary>
        public double FootnoteLineHeight { get; set; } = 18;
        
        /// <summary>
        /// גובה כותרת מערכת הערות
        /// </summary>
        public double FootnoteHeaderHeight { get; set; } = 25;
        
        /// <summary>
        /// גובה קו הפרדה
        /// </summary>
        public double SeparatorHeight { get; set; } = 15;
        
        // ═══════════════════════════════════════════════════════════
        // גובה זמין לחישובים
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// גובה זמין בתחילת החישוב (לאחר הכותרת)
        /// </summary>
        public double AvailableHeight => PageHeight - HeaderHeight - MarginOuter;
        
        /// <summary>
        /// גובה שנוצל כבר בעמוד זה
        /// </summary>
        public double UsedHeight { get; set; } = 0;
        
        /// <summary>
        /// גובה נותר לחישוב מתי לחתוך עמוד
        /// </summary>
        public double RemainingHeight => AvailableHeight - UsedHeight;
        
        /// <summary>
        /// האם העמוד מלא
        /// </summary>
        public bool IsFull => RemainingHeight <= 0;
        
        // ═══════════════════════════════════════════════════════════
        // שיטות עזר
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// מחשב סך הגובה שנדרש לכל התוכן בעמוד זה
        /// </summary>
        public double CalculateTotalHeight()
        {
            double total = 0;
            
            // טקסט ראשי — הגבוה מבין הטורים
            int rightLines = MainContentColumnRight?.Count ?? 0;
            int leftLines = MainContentColumnLeft?.Count ?? 0;
            int maxMainLines = System.Math.Max(rightLines, leftLines);
            total += maxMainLines * MainLineHeight;
            
            // הערות מערכת 1
            if ((FootnoteAColumnRight?.Count ?? 0) + (FootnoteAColumnLeft?.Count ?? 0) > 0)
            {
                total += FootnoteHeaderHeight + SeparatorHeight;
                int aRight = FootnoteAColumnRight?.Count ?? 0;
                int aLeft = FootnoteAColumnLeft?.Count ?? 0;
                total += System.Math.Max(aRight, aLeft) * FootnoteLineHeight;
            }
            
            // הערות מערכת 2
            if ((FootnoteBColumnRight?.Count ?? 0) + (FootnoteBColumnLeft?.Count ?? 0) > 0)
            {
                total += FootnoteHeaderHeight + SeparatorHeight;
                int bRight = FootnoteBColumnRight?.Count ?? 0;
                int bLeft = FootnoteBColumnLeft?.Count ?? 0;
                total += System.Math.Max(bRight, bLeft) * FootnoteLineHeight;
            }
            
            // הערות מערכת 3
            if ((FootnoteCColumnRight?.Count ?? 0) + (FootnoteCColumnLeft?.Count ?? 0) > 0)
            {
                total += FootnoteHeaderHeight + SeparatorHeight;
                int cRight = FootnoteCColumnRight?.Count ?? 0;
                int cLeft = FootnoteCColumnLeft?.Count ?? 0;
                total += System.Math.Max(cRight, cLeft) * FootnoteLineHeight;
            }
            
            return total;
        }
        
        /// <summary>
        /// מחזיר סיכום של העמוד לצורך דיבוג
        /// </summary>
        public override string ToString()
        {
            int totalMain = (MainContentColumnRight?.Count ?? 0) + (MainContentColumnLeft?.Count ?? 0);
            int totalA = (FootnoteAColumnRight?.Count ?? 0) + (FootnoteAColumnLeft?.Count ?? 0);
            int totalB = (FootnoteBColumnRight?.Count ?? 0) + (FootnoteBColumnLeft?.Count ?? 0);
            int totalC = (FootnoteCColumnRight?.Count ?? 0) + (FootnoteCColumnLeft?.Count ?? 0);
            
            return $"עמוד {PageNumber} ({RunningHeaderCenter}): " +
                   $"טקסט: {totalMain} שורות, " +
                   $"הערות א': {totalA}, הערות ב': {totalB}, הערות ג': {totalC}, " +
                   $"גובה: {CalculateTotalHeight():F0}px/{AvailableHeight:F0}px";
        }
        
        // ═══════════════════════════════════════════════════════════
        // תאימות לאחור עם PaginationEngine הישן
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// שומר גובה בעמוד (לתאימות לאחור עם PaginationEngine הישן)
        /// </summary>
        public void ReserveHeight(double height)
        {
            UsedHeight += height;
        }
    }
    
    /// <summary>
    /// מיקום פסוק בטקסט — לכותרת רצה
    /// </summary>
    public class VerseLocation
    {
        /// <summary>
        /// שם הספר
        /// </summary>
        public string BookName { get; set; } = string.Empty;
        
        /// <summary>
        /// שם הפרשה
        /// </summary>
        public string ParashaName { get; set; } = string.Empty;
        
        /// <summary>
        /// מספר פרק
        /// </summary>
        public int Chapter { get; set; }
        
        /// <summary>
        /// מספר פסוק
        /// </summary>
        public int Verse { get; set; }
        
        /// <summary>
        /// מזהה ייחודי לפסוק
        /// </summary>
        public string VerseId => $"{BookName}_{Chapter}_{Verse}";
    }
}
