using System;
using System.Collections.Generic;
using System.Linq;
using UI_Application.Models;

namespace UI_Application.Services
{
    /// <summary>
    /// מנוע עימוד - מחלק טקסט לעמודים עם ניהול הערות שוליים
    /// </summary>
    public class PaginationEngine
    {
        private readonly double _pageWidth;
        private readonly double _pageHeight;
        private readonly double _lineHeight;
        private readonly double _footnoteHeight;

        public PaginationEngine(
            double pageWidth = 800,
            double pageHeight = 600,
            double lineHeight = 30,
            double footnoteHeight = 20)
        {
            _pageWidth = pageWidth;
            _pageHeight = pageHeight;
            _lineHeight = lineHeight;
            _footnoteHeight = footnoteHeight;
        }

        /// <summary>
        /// מחלק טקסט גולמי לעמודים עם הערות שוליים
        /// </summary>
        public List<PageModel> Paginate(string rawText)
        {
            var pages = new List<PageModel>();
            
            if (string.IsNullOrWhiteSpace(rawText))
                return pages;

            // הפרדת טקסט ראשי מהערות
            var (mainText, footnotesA, footnotesB, footnotesC) = TextHelper.ExtractContentAndFootnotes(rawText);
            
            // חלוקה לעמודים
            var lines = mainText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var currentPage = CreateNewPage(1);
            
            int lineIndex = 0;
            int footnoteAIndex = 0;
            int footnoteBIndex = 0;
            int footnoteCIndex = 0;

            while (lineIndex < lines.Length)
            {
                var line = lines[lineIndex];
                double lineHeight = CalculateLineHeight(line);

                // בדיקה אם יש מקום בעמוד הנוכחי
                if (currentPage.RemainingHeight >= lineHeight)
                {
                    // הוספת שורה לעמוד הנוכחי
                    currentPage.AllMainLines.Add(line);
                    currentPage.ReserveHeight(lineHeight);

                    // הוספת הערות שוליים מקושרות לשורה זו
                    AddFootnotesForLine(currentPage, ref footnoteAIndex, ref footnoteBIndex, ref footnoteCIndex, footnotesA, footnotesB, footnotesC);

                    lineIndex++;
                }
                else
                {
                    // העמוד מלא - שמירה ויצירת עמוד חדש
                    pages.Add(currentPage);
                    currentPage = CreateNewPage(pages.Count + 1);
                }
            }

            // הוספת העמוד האחרון אם לא ריק
            if (currentPage.AllMainLines.Count > 0)
            {
                pages.Add(currentPage);
            }

            return pages;
        }

        /// <summary>
        /// יוצר עמוד חדש עם הגדרות ברירת מחדל
        /// </summary>
        private PageModel CreateNewPage(int pageNumber)
        {
            return new PageModel
            {
                PageNumber = pageNumber,
                PageWidth = _pageWidth,
                PageHeight = _pageHeight,
                AllMainLines = new List<string>(),
                AllFootnotesA = new List<string>(),
                AllFootnotesB = new List<string>(),
                AllFootnotesC = new List<string>()
            };
        }

        /// <summary>
        /// מחשב גובה שורה לפי תוכן
        /// </summary>
        private double CalculateLineHeight(string line)
        {
            // הערכה פשוטה: שורה רגילה + תוספת לשורות ארוכות
            double baseHeight = _lineHeight;
            
            // אם השורה ארוכה מאוד, נוסיף גובה
            if (line.Length > 50)
                baseHeight += _lineHeight * 0.5;

            return baseHeight;
        }

        /// <summary>
        /// מוסיף הערות שוליים לעמוד - כל ההערות שנותרו
        /// </summary>
        private void AddFootnotesForLine(
            PageModel page, 
            ref int footnoteAIndex, 
            ref int footnoteBIndex,
            ref int footnoteCIndex, 
            List<string> footnotesA, 
            List<string> footnotesB,
            List<string> footnotesC)
        {
            // הוספת כל הערות א' שנותרו (כולן לעמוד זה)
            while (footnoteAIndex < footnotesA.Count)
            {
                page.AllFootnotesA.Add(footnotesA[footnoteAIndex]);
                footnoteAIndex++;
            }

            // הוספת כל הערות ב' שנותרו
            while (footnoteBIndex < footnotesB.Count)
            {
                page.AllFootnotesB.Add(footnotesB[footnoteBIndex]);
                footnoteBIndex++;
            }

            // הוספת כל הערות ג' שנותרו
            while (footnoteCIndex < footnotesC.Count)
            {
                page.AllFootnotesC.Add(footnotesC[footnoteCIndex]);
                footnoteCIndex++;
            }
        }

        /// <summary>
        /// מחזיר סיכום של העימוד
        /// </summary>
        public string GetPaginationSummary(List<PageModel> pages)
        {
            if (pages.Count == 0)
                return "אין עמודים";

            int totalChars = pages.Sum(p => p.AllMainLines?.Count ?? 0);
            int totalFootnotesA = pages.Sum(p => p.AllFootnotesA?.Count ?? 0);
            int totalFootnotesB = pages.Sum(p => p.AllFootnotesB?.Count ?? 0);
            int totalFootnotesC = pages.Sum(p => p.AllFootnotesC?.Count ?? 0);

            return $"עמודים: {pages.Count}, תווים: {totalChars}, " +
                   $"הערות א': {totalFootnotesA}, הערות ב': {totalFootnotesB}, הערות ג': {totalFootnotesC}";
        }
    }
}
