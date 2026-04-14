using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using UI_Application.Constants;
using UI_Application.Models;

namespace UI_Application.Services
{
    /// <summary>
    /// מנוע ניתוח טקסט — מפריד טקסט ראשי משלוש מערכות הערות
    /// </summary>
    public class TextParser
    {
        private readonly Dictionary<string, double> _widthCache = new();
        private readonly Visual? _visual;
        
        public TextParser(Visual? visual = null)
        {
            _visual = visual;
        }
        
        /// <summary>
        /// ניתוח הטקסט הגולמי — הפרדה לטקסט ראשי והערות
        /// </summary>
        public ParsedDocument Parse(string rawText)
        {
            var doc = new ParsedDocument { RawText = rawText };
            var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // זיהוי שם ספר
                if (trimmed.StartsWith(LayoutConstants.MARKER_BOOK_NAME))
                {
                    doc.BookName = trimmed.Substring(LayoutConstants.MARKER_BOOK_NAME.Length).Trim();
                    continue;
                }
                
                // זיהוי שם פרשה
                if (trimmed.StartsWith(LayoutConstants.MARKER_PARASHA))
                {
                    doc.ParashaName = trimmed.Substring(LayoutConstants.MARKER_PARASHA.Length).Trim();
                    continue;
                }
                
                // זיהוי הערות מערכת A ($$)
                if (trimmed.StartsWith(LayoutConstants.MARKER_FOOTNOTE_A))
                {
                    doc.FootnotesA.Add(trimmed.Substring(LayoutConstants.MARKER_FOOTNOTE_A.Length).Trim());
                    continue;
                }
                
                // זיהוי הערות מערכת B (%%)
                if (trimmed.StartsWith(LayoutConstants.MARKER_FOOTNOTE_B))
                {
                    doc.FootnotesB.Add(trimmed.Substring(LayoutConstants.MARKER_FOOTNOTE_B.Length).Trim());
                    continue;
                }
                
                // זיהוי הערות מערכת C (&&)
                if (trimmed.StartsWith(LayoutConstants.MARKER_FOOTNOTE_C))
                {
                    doc.FootnotesC.Add(trimmed.Substring(LayoutConstants.MARKER_FOOTNOTE_C.Length).Trim());
                    continue;
                }
                
                // כל השאר — טקסט ראשי
                doc.MainLines.Add(trimmed);
            }
            
            return doc;
        }
        
        /// <summary>
        /// הסרת ניקוד וטעמים ממחרוזת (לצורך מדידה מדויקת)
        /// טווח Unicode: 0x05B0–0x05C7
        /// </summary>
        public string RemoveNikkud(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                int code = c;
                if (code < LayoutConstants.NIKUD_START || code > LayoutConstants.NIKUD_END)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// בדיקה אם תו הוא ניקוד
        /// </summary>
        public bool IsNikkudChar(char c)
        {
            int code = c;
            return code >= LayoutConstants.NIKUD_START && code <= LayoutConstants.NIKUD_END;
        }
        
        /// <summary>
        /// מדידת רוחב טקסט בפיקסלים — ללא ניקוד!
        /// </summary>
        public double MeasureTextWidth(string text, double fontSize, string fontFamily)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            
            // מפתח לקאש
            var cacheKey = $"{text}|{fontSize}|{fontFamily}";
            if (_widthCache.TryGetValue(cacheKey, out double cached))
                return cached;
            
            // הסרת ניקוד לפני מדידה
            var textWithoutNikkud = RemoveNikkud(text);
            
            try
            {
                var typeface = new Typeface(
                    new FontFamily(fontFamily),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal);
                
                var formattedText = new FormattedText(
                    textWithoutNikkud,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.RightToLeft,
                    typeface,
                    fontSize * (96.0 / 72.0), // המרה מנקודות לפיקסלים
                    Brushes.Black,
                    1.0); // pixelsPerDip
                
                double width = formattedText.Width;
                _widthCache[cacheKey] = width;
                return width;
            }
            catch
            {
                return textWithoutNikkud.Length * fontSize * 0.6; // fallback
            }
        }
        
        /// <summary>
        /// שבירת פסקאות לשורות לפי רוחב מקסימלי
        /// </summary>
        public List<string> WrapLinesToWidth(List<string> paragraphs, double maxWidth, 
                                              double fontSize, string fontFamily)
        {
            var result = new List<string>();
            
            foreach (var paragraph in paragraphs)
            {
                var words = paragraph.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var currentLine = new StringBuilder();
                bool isFirstWord = true;
                
                foreach (var word in words)
                {
                    var testLine = isFirstWord ? word : currentLine + " " + word;
                    var width = MeasureTextWidth(testLine, fontSize, fontFamily);
                    
                    if (width <= maxWidth)
                    {
                        // המילה נכנסת
                        if (!isFirstWord)
                            currentLine.Append(" ");
                        currentLine.Append(word);
                        isFirstWord = false;
                    }
                    else
                    {
                        // המילה לא נכנסה — סיום השורה והתחלת שורה חדשה
                        if (currentLine.Length > 0)
                        {
                            result.Add(currentLine.ToString());
                            currentLine.Clear();
                        }
                        currentLine.Append(word);
                        isFirstWord = false;
                    }
                }
                
                // הוספת השורה האחרונה עם סימון "שורה אחרונה"
                if (currentLine.Length > 0)
                {
                    result.Add(currentLine.ToString() + "\x00A4"); // תו בקרה פנימי לסימון שורה אחרונה
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// בדיקה אם שורה היא שורה אחרונה של פסקה
        /// </summary>
        public bool IsLastLine(string line)
        {
            return line.EndsWith("\x00A4");
        }
        
        /// <summary>
        /// הסרת סימון שורה אחרונה
        /// </summary>
        public string RemoveLastLineMarker(string line)
        {
            if (line.EndsWith("\x00A4"))
                return line.Substring(0, line.Length - 1);
            return line;
        }
        
        /// <summary>
        /// מציאת מספרי הערות בטקסט
        /// </summary>
        public List<int> FindFootnoteReferences(string text, string prefix)
        {
            var result = new List<int>();
            int i = 0;
            while (i < text.Length)
            {
                int idx = text.IndexOf(prefix, i);
                if (idx == -1) break;
                
                // קריאת המספר
                int numStart = idx + prefix.Length;
                int numEnd = numStart;
                while (numEnd < text.Length && char.IsDigit(text[numEnd]))
                    numEnd++;
                
                if (numEnd > numStart && int.TryParse(text.Substring(numStart, numEnd - numStart), out int num))
                {
                    result.Add(num);
                }
                
                i = numEnd;
            }
            return result;
        }
    }
}
