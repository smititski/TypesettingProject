using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using UI_Application.Models;

namespace UI_Application.Services
{
    /// <summary>
    /// מנוע עימוד דו-טורי עם אלגוריתם Greedy Fit
    /// Traditional Scholarly Page Layout — 3-Commentary Edition
    /// </summary>
    public class ColumnLayoutEngine
    {
        private readonly Visual _visual;
        private readonly double _pixelsPerDip;
        private readonly FontFamily _mainFont;
        private readonly FontFamily _footnoteFont;
        
        // קבועים לחישובי גובה
        private const double MAIN_LINE_HEIGHT_MULTIPLIER = 1.5;
        private const double FOOTNOTE_LINE_HEIGHT_MULTIPLIER = 1.4;
        private const double SAFETY_BUFFER = 0.93; // 93% ריפוד ביטחון
        
        /// <summary>
        /// יוצר מופע חדש של מנוע העימוד
        /// </summary>
        public ColumnLayoutEngine(Visual visual)
        {
            _visual = visual;
            _pixelsPerDip = VisualTreeHelper.GetDpi(visual).PixelsPerDip;
            _mainFont = new FontFamily("Frank Ruehl CLM");
            _footnoteFont = new FontFamily("Frank Ruehl CLM");
        }
        
        /// <summary>
        /// מנתח מסמך ומחלק אותו לעמודים דו-טוריים
        /// </summary>
        public List<PageModel> Paginate(ParsedDocument document)
        {
            var pages = new List<PageModel>();
            int currentLine = 0;
            int pageNumber = 1;
            
            while (currentLine < document.MainLines.Count)
            {
                // נסה ליצור עמוד החל מהשורה הנוכחית
                var page = TryFitPage(document, currentLine, pageNumber);
                
                // אם לא הצלחנו לשים אף שורה — חייבים לשים לפחות אחת
                if (page.AllMainLines.Count == 0 && currentLine < document.MainLines.Count)
                {
                    page = ForceSingleLinePage(document, currentLine, pageNumber);
                }
                
                // הגדר כותרת רצה
                SetRunningHeader(page, document, currentLine);
                
                // חלק תוכן לשני טורים
                DistributeToColumns(page);
                
                pages.Add(page);
                currentLine += page.AllMainLines.Count;
                pageNumber++;
            }
            
            return pages;
        }
        
        /// <summary>
        /// מנסה ליצור עמוד עם כמה שיותר שורות שמתאימות
        /// Greedy Fit Algorithm
        /// </summary>
        private PageModel TryFitPage(ParsedDocument doc, int startLine, int pageNumber)
        {
            var page = new PageModel { PageNumber = pageNumber };
            double availableHeight = page.AvailableHeight * SAFETY_BUFFER;
            
            int bestFit = 0;
            
            // נסה להוסיף שורות אחת-אחת עד שנגמר הגובה
            for (int count = 1; startLine + count <= doc.MainLines.Count; count++)
            {
                // אסוף הערות השייכות לטווח שורות זה
                var footnotesA = GetFootnotesForRange(doc, startLine, count, doc.FootnoteALocations);
                var footnotesB = GetFootnotesForRange(doc, startLine, count, doc.FootnoteBLocations);
                var footnotesC = GetFootnotesForRange(doc, startLine, count, doc.FootnoteCLocations);
                
                // בדוק שלא חותכים הערה באמצע "בלוק" (פסוק)
                if (count > 1 && IsCuttingMidVerse(doc, startLine, count))
                {
                    continue; // נסה להוסיף עוד שורה
                }
                
                // חשב גובה כולל
                double totalHeight = CalculateTotalHeight(
                    count, 
                    footnotesA.Count, 
                    footnotesB.Count, 
                    footnotesC.Count,
                    page.ColumnWidth);
                
                if (totalHeight <= availableHeight)
                {
                    bestFit = count;
                }
                else
                {
                    break; // חרגנו מגובה העמוד
                }
            }
            
            // אם לא מצאנו התאמה — נסה בלי הבדיקה על חיתוך פסוק
            if (bestFit == 0)
            {
                for (int count = 1; startLine + count <= doc.MainLines.Count && count <= 3; count++)
                {
                    var footnotesA = GetFootnotesForRange(doc, startLine, count, doc.FootnoteALocations);
                    var footnotesB = GetFootnotesForRange(doc, startLine, count, doc.FootnoteBLocations);
                    var footnotesC = GetFootnotesForRange(doc, startLine, count, doc.FootnoteCLocations);
                    
                    double totalHeight = CalculateTotalHeight(
                        count, 
                        footnotesA.Count, 
                        footnotesB.Count, 
                        footnotesC.Count,
                        page.ColumnWidth);
                    
                    if (totalHeight <= availableHeight)
                    {
                        bestFit = count;
                        break;
                    }
                }
            }
            
            // מלא את העמוד עם מה שמצאנו
            if (bestFit > 0)
            {
                page.AllMainLines = doc.MainLines.Skip(startLine).Take(bestFit).ToList();
                page.AllFootnotesA = GetFootnotesForRange(doc, startLine, bestFit, doc.FootnoteALocations);
                page.AllFootnotesB = GetFootnotesForRange(doc, startLine, bestFit, doc.FootnoteBLocations);
                page.AllFootnotesC = GetFootnotesForRange(doc, startLine, bestFit, doc.FootnoteCLocations);
                
                // מלא מיפויי שורות
                for (int i = 0; i < bestFit; i++)
                {
                    int lineIndex = startLine + i;
                    if (doc.LineToVerseLocation.ContainsKey(lineIndex))
                    {
                        page.LineToVerseLocation[i] = doc.LineToVerseLocation[lineIndex];
                    }
                }
            }
            
            return page;
        }
        
        /// <summary>
        /// יוצר עמוד עם שורה אחת בכל מקרה (למקרי קצה)
        /// </summary>
        private PageModel ForceSingleLinePage(ParsedDocument doc, int lineIndex, int pageNumber)
        {
            var page = new PageModel { PageNumber = pageNumber };
            page.AllMainLines = new List<string> { doc.MainLines[lineIndex] };
            page.AllFootnotesA = GetFootnotesForRange(doc, lineIndex, 1, doc.FootnoteALocations);
            page.AllFootnotesB = GetFootnotesForRange(doc, lineIndex, 1, doc.FootnoteBLocations);
            page.AllFootnotesC = GetFootnotesForRange(doc, lineIndex, 1, doc.FootnoteCLocations);
            
            if (doc.LineToVerseLocation.ContainsKey(lineIndex))
            {
                page.LineToVerseLocation[0] = doc.LineToVerseLocation[lineIndex];
            }
            
            return page;
        }
        
        /// <summary>
        /// חלק את התוכן לשני טורים (ימין ושמאל)
        /// </summary>
        private void DistributeToColumns(PageModel page)
        {
            int totalLines = page.AllMainLines.Count;
            if (totalLines == 0) return;
            
            // חלק שווה: חצי לימין, חצי לשמאל
            int rightCount = (int)Math.Ceiling(totalLines / 2.0);
            int leftCount = totalLines - rightCount;
            
            page.MainContentColumnRight = page.AllMainLines.Take(rightCount).ToList();
            page.MainContentColumnLeft = page.AllMainLines.Skip(rightCount).Take(leftCount).ToList();
            
            // חלק הערות מערכת 1
            DistributeFootnotes(page.AllFootnotesA, page.FootnoteAColumnRight, page.FootnoteAColumnLeft);
            
            // חלק הערות מערכת 2
            DistributeFootnotes(page.AllFootnotesB, page.FootnoteBColumnRight, page.FootnoteBColumnLeft);
            
            // חלק הערות מערכת 3
            DistributeFootnotes(page.AllFootnotesC, page.FootnoteCColumnRight, page.FootnoteCColumnLeft);
        }
        
        /// <summary>
        /// מחלק רשימת הערות לשני טורים
        /// </summary>
        private void DistributeFootnotes(List<string> allNotes, List<string> rightColumn, List<string> leftColumn)
        {
            rightColumn.Clear();
            leftColumn.Clear();
            
            if (allNotes.Count == 0) return;
            
            int rightCount = (int)Math.Ceiling(allNotes.Count / 2.0);
            
            rightColumn.AddRange(allNotes.Take(rightCount));
            leftColumn.AddRange(allNotes.Skip(rightCount));
        }
        
        /// <summary>
        /// מחשב גובה כולל נדרש לעמוד
        /// </summary>
        private double CalculateTotalHeight(int mainLines, int footnotesACount, int footnotesBCount, int footnotesCCount, double columnWidth)
        {
            double total = 0;
            
            // גובה טקסט ראשי
            total += mainLines * 28 * MAIN_LINE_HEIGHT_MULTIPLIER;
            
            // גובה הערות מערכת 1
            if (footnotesACount > 0)
            {
                total += 25; // כותרת
                total += 15; // קו הפרדה
                total += MeasureFootnoteHeight(footnotesACount, columnWidth);
            }
            
            // גובה הערות מערכת 2
            if (footnotesBCount > 0)
            {
                total += 25; // כותרת
                total += 15; // קו הפרדה
                total += MeasureFootnoteHeight(footnotesBCount, columnWidth);
            }
            
            // גובה הערות מערכת 3
            if (footnotesCCount > 0)
            {
                total += 25; // כותרת
                total += 15; // קו הפרדה
                total += MeasureFootnoteHeight(footnotesCCount, columnWidth);
            }
            
            return total;
        }
        
        /// <summary>
        /// מודד גובה הערות בפורמט run-on
        /// </summary>
        private double MeasureFootnoteHeight(int footnoteCount, double columnWidth)
        {
            if (footnoteCount == 0) return 0;
            
            // הערכה: 2 הערות = שורה אחת, מחולק לשני טורים
            double linesPerColumn = Math.Ceiling(footnoteCount / 4.0);
            return linesPerColumn * 13 * FOOTNOTE_LINE_HEIGHT_MULTIPLIER;
        }
        
        /// <summary>
        /// מודד גובה מדויק של הערות בפורמט run-on באמצעות FormattedText
        /// </summary>
        public double MeasureRunOnFootnotesHeight(List<string> footnotes, double columnWidth, double fontSize)
        {
            if (footnotes.Count == 0) return 0;
            
            // בנה את הטקסט המלא כפי שייראה בפועל
            string fullText = BuildRunOnText(footnotes);
            
            // מדוד כמה שורות ייקח בטור אחד
            int lineCount = MeasureLineCount(fullText, columnWidth, fontSize);
            
            // הטקסט מתחלק לשני טורים
            int linesPerColumn = (int)Math.Ceiling(lineCount / 2.0);
            
            // גובה = שורות × גובה שורה + ריפוד
            double lineHeight = fontSize * 1.4;
            return (linesPerColumn * lineHeight) + 10;
        }
        
        /// <summary>
        /// בונה מחרוזת run-on מדויקת
        /// </summary>
        private string BuildRunOnText(List<string> footnotes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < footnotes.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append($"({i + 1}) ");
                sb.Append(footnotes[i]);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// מודד מספר שורות שיטקסט ייקח ברוחב מסוים
        /// </summary>
        public int MeasureLineCount(string text, double maxWidth, double fontSize)
        {
            var typeface = new Typeface(_footnoteFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            
            var formatted = new FormattedText(
                text,
                System.Globalization.CultureInfo.GetCultureInfo("he-IL"),
                System.Windows.FlowDirection.RightToLeft,
                typeface,
                fontSize,
                Brushes.Black,
                _pixelsPerDip);
            
            formatted.MaxTextWidth = maxWidth;
            formatted.MaxTextHeight = double.MaxValue;
            
            double lineHeight = formatted.LineHeight > 0 ? formatted.LineHeight : fontSize * 1.4;
            int lineCount = (int)Math.Ceiling(formatted.Height / lineHeight);
            
            return Math.Max(1, lineCount);
        }
        
        /// <summary>
        /// מודד רוחב מילה בודדת
        /// </summary>
        public double MeasureWordWidth(string word, double fontSize)
        {
            string clean = RemoveNikud(word);
            
            var typeface = new Typeface(_mainFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            
            var formatted = new FormattedText(
                clean,
                System.Globalization.CultureInfo.GetCultureInfo("he-IL"),
                System.Windows.FlowDirection.RightToLeft,
                typeface,
                fontSize,
                Brushes.Black,
                _pixelsPerDip);
            
            return formatted.WidthIncludingTrailingWhitespace;
        }
        
        /// <summary>
        /// מסיר תווי ניקוד למדידה מדויקת
        /// </summary>
        private string RemoveNikud(string text)
        {
            return new string(text.Where(c => c < '\u05B0' || c > '\u05C7').ToArray());
        }
        
        /// <summary>
        /// בדוק אם חיתוך בשורה נתונה חותך באמצע פסוק
        /// </summary>
        private bool IsCuttingMidVerse(ParsedDocument doc, int startLine, int count)
        {
            int lastLine = startLine + count - 1;
            int nextLine = startLine + count;
            
            if (nextLine >= doc.MainLines.Count) return false;
            
            if (!doc.LineToVerseLocation.ContainsKey(lastLine) || 
                !doc.LineToVerseLocation.ContainsKey(nextLine))
                return false;
            
            string lastVerseId = doc.LineToVerseLocation[lastLine].VerseId;
            string nextVerseId = doc.LineToVerseLocation[nextLine].VerseId;
            
            return lastVerseId == nextVerseId;
        }
        
        /// <summary>
        /// אוסף הערות השייכות לטווח שורות
        /// </summary>
        private List<string> GetFootnotesForRange(ParsedDocument doc, int startLine, int count, Dictionary<int, List<int>> footnoteLocations)
        {
            var result = new List<string>();
            var addedIndices = new HashSet<int>();
            
            for (int i = 0; i < count; i++)
            {
                int lineIndex = startLine + i;
                if (footnoteLocations.ContainsKey(lineIndex))
                {
                    foreach (int footnoteIndex in footnoteLocations[lineIndex])
                    {
                        if (!addedIndices.Contains(footnoteIndex))
                        {
                            // מצא את ההערה המתאימה
                            string note = GetFootnoteByIndex(doc, footnoteIndex);
                            if (!string.IsNullOrEmpty(note))
                            {
                                result.Add(note);
                                addedIndices.Add(footnoteIndex);
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// מוצא הערה לפי אינדקס
        /// </summary>
        private string GetFootnoteByIndex(ParsedDocument doc, int index)
        {
            // זהו מיפוי פשוט — במימוש אמיתי צריך לאחסן את כל ההערות במקום אחד
            // כרגע נניח שהאינדקס מתאים למיקום ברשימה
            return $"הערה מספר {index}";
        }
        
        /// <summary>
        /// מגדיר את כותרת הרצה לעמוד
        /// </summary>
        private void SetRunningHeader(PageModel page, ParsedDocument doc, int startLine)
        {
            // ברירת מחדל — שם הספר והפרשה מהשורה הראשונה בעמוד
            if (doc.LineToVerseLocation.ContainsKey(startLine))
            {
                var location = doc.LineToVerseLocation[startLine];
                page.RunningHeaderCenter = location.ParashaName;
                page.RunningHeaderRight = $"{location.BookName} {location.Chapter}:{location.Verse}";
                page.RunningHeaderLeft = $"עמוד {page.PageNumber}";
            }
            else
            {
                page.RunningHeaderCenter = "—";
                page.RunningHeaderRight = "—";
                page.RunningHeaderLeft = $"עמוד {page.PageNumber}";
            }
        }
    }
    
    /// <summary>
    /// מסמך מנותח — נתוני קלט למנוע העימוד
    /// </summary>
    public class ParsedDocument
    {
        /// <summary>
        /// שורות טקסט ראשי
        /// </summary>
        public List<string> MainLines { get; set; } = new List<string>();
        
        /// <summary>
        /// מיפוי: שורה → מיקום פסוק
        /// </summary>
        public Dictionary<int, VerseLocation> LineToVerseLocation { get; set; } = new Dictionary<int, VerseLocation>();
        
        /// <summary>
        /// מיפוי: שורה → אינדקסי הערות מערכת 1
        /// </summary>
        public Dictionary<int, List<int>> FootnoteALocations { get; set; } = new Dictionary<int, List<int>>();
        
        /// <summary>
        /// מיפוי: שורה → אינדקסי הערות מערכת 2
        /// </summary>
        public Dictionary<int, List<int>> FootnoteBLocations { get; set; } = new Dictionary<int, List<int>>();
        
        /// <summary>
        /// מיפוי: שורה → אינדקסי הערות מערכת 3
        /// </summary>
        public Dictionary<int, List<int>> FootnoteCLocations { get; set; } = new Dictionary<int, List<int>>();
        
        /// <summary>
        /// כל הערות מערכת 1
        /// </summary>
        public List<string> AllFootnotesA { get; set; } = new List<string>();
        
        /// <summary>
        /// כל הערות מערכת 2
        /// </summary>
        public List<string> AllFootnotesB { get; set; } = new List<string>();
        
        /// <summary>
        /// כל הערות מערכת 3
        /// </summary>
        public List<string> AllFootnotesC { get; set; } = new List<string>();
    }
}
