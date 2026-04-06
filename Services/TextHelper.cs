using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UI_Application.Services
{
    /// <summary>
    /// כלי עזר לעיבוד טקסט - הפרדת תוכן ראשי מהערות שוליים
    /// </summary>
    public static class TextHelper
    {
        /// <summary>
        /// מפריד טקסט גולמי לתוכן ראשי והערות שוליים
        /// </summary>
        /// <param name="rawText">הטקסט הגולמי עם הערות</param>
        /// <returns>(תוכן ראשי, הערות א', הערות ב')</returns>
        public static (string mainText, List<string> footnotesA, List<string> footnotesB, List<string> footnotesC) ExtractContentAndFootnotes(string rawText)
        {
            var fA = new List<string>();
            var fB = new List<string>();
            var fC = new List<string>();
            
            // חילוץ הערות סוג א' - $$
            string mainText = ExtractByTag(rawText, "$$", fA);
            // חילוץ הערות סוג ב' - %%
            mainText = ExtractByTag(mainText, "%%", fB);
            // חילוץ הערות סוג ג' - &&
            mainText = ExtractByTag(mainText, "&&", fC);
            
            // ניקוי רווחים מיותרים
            mainText = Regex.Replace(mainText, @"\s+", " ").Trim();
            
            return (mainText, fA, fB, fC);
        }
        
        /// <summary>
        /// מחלץ תוכן לפי תג כפול ומחזיר את הטקסט הנותר
        /// </summary>
        private static string ExtractByTag(string text, string tag, List<string> results)
        {
            var builder = new System.Text.StringBuilder();
            int i = 0;
            
            while (i < text.Length)
            {
                // בדיקה אם יש תג פותח
                if (i + tag.Length <= text.Length && text.Substring(i, tag.Length) == tag)
                {
                    // מצאנו תג פותח, חפש את התג הסוגר
                    int start = i + tag.Length;
                    int end = text.IndexOf(tag, start);
                    
                    if (end > 0) // מצאנו תג סוגר
                    {
                        // גזירת התוכן
                        string content = text.Substring(start, end - start).Trim();
                        results.Add(content);
                        
                        // דלג מעל כל הקטע כולל התגים
                        i = end + tag.Length;
                        continue;
                    }
                }
                
                // לא תג - הוסף לטקסט הראשי
                builder.Append(text[i]);
                i++;
            }
            
            return builder.ToString();
        }

        /// <summary>
        /// מחלק טקסט ארוך לשורות לפי אורך מקסימלי
        /// </summary>
        public static List<string> WrapText(string text, int maxCharsPerLine = 50)
        {
            var lines = new List<string>();
            
            if (string.IsNullOrWhiteSpace(text))
                return lines;

            var words = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new System.Text.StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxCharsPerLine)
                {
                    // השורה מלאה - שמירה והתחלת שורה חדשה
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString().Trim());
                        currentLine.Clear();
                    }
                }

                if (currentLine.Length > 0)
                    currentLine.Append(" ");
                currentLine.Append(word);
            }

            // הוספת השורה האחרונה
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            return lines;
        }

        /// <summary>
        /// מחלק טקסט לפסקאות
        /// </summary>
        public static List<string> SplitToParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(p => p.Trim())
                       .Where(p => !string.IsNullOrWhiteSpace(p))
                       .ToList();
        }

        /// <summary>
        /// מנקה טקסט מתווים מיוחדים ומכין לעימוד
        /// </summary>
        public static string CleanTextForLayout(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // הסרת תווי שליטה מלבד שורות חדשות
            var cleaned = new string(text.Where(c => !char.IsControl(c) || c == '\n' || c == '\r').ToArray());
            
            // ניקוי רווחים מיותרים
            cleaned = Regex.Replace(cleaned, @"[ \t]+", " ");
            cleaned = Regex.Replace(cleaned, @"\n\s*\n\s*\n", "\n\n"); // מקסימום שני שורות ריקות ברצף

            return cleaned.Trim();
        }

        /// <summary>
        /// מחזיר סיכום של הטקסט לדיבוג
        /// </summary>
        public static string GetTextSummary(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "טקסט ריק";

            var (mainText, footnotesA, footnotesB, footnotesC) = ExtractContentAndFootnotes(text);
            
            return $"תווים: {text.Length}, " +
                   $"תוכן ראשי: {mainText.Length}, " +
                   $"הערות א': {footnotesA.Count}, " +
                   $"הערות ב': {footnotesB.Count}, " +
                   $"הערות ג': {footnotesC.Count}";
        }
    }
}
