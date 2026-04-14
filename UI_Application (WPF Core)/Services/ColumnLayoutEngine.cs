using System;
using System.Collections.Generic;
using System.Linq;
using UI_Application.Constants;
using UI_Application.Models;

namespace UI_Application.Services
{
    /// <summary>
    /// מנוע עימוד דו-טורי עם אלגוריתם Greedy Fit — גרסה חדשה
    /// Traditional Scholarly Page Layout — 3-Commentary Edition
    /// </summary>
    public class ColumnLayoutEngine
    {
        private readonly TextParser _textParser;
        private readonly HeightCalculator _heightCalc;
        private ParsedDocument _doc = null!;
        
        // רשימות מוכללות (wrapped) לפי רוחב הטור
        private List<string> _mainWrapped = new();
        private List<string> _footnotesA_Wrapped = new();
        private List<string> _footnotesB_Wrapped = new();
        private List<string> _footnotesC_Wrapped = new();
        
        public ColumnLayoutEngine(TextParser textParser)
        {
            _textParser = textParser;
            _heightCalc = new HeightCalculator(textParser);
        }
        
        /// <summary>
        /// העימוד הראשי — שלב 4: Greedy Fit Algorithm
        /// </summary>
        public List<PageModel> Paginate(ParsedDocument document)
        {
            _doc = document;
            
            // שלב 1: ריווח כל הטקסט לרוחב הטור
            PreWrapAllText();
            
            var pages = new List<PageModel>();
            int currentLine = 0;
            int pageNumber = 1;
            
            // שלב 2: לולאת עמודים
            while (currentLine < _mainWrapped.Count)
            {
                var page = TryFitPage(currentLine, pageNumber);
                
                // אם לא הצלחנו — לפחות שורה אחת
                if (page.MainRight.Count == 0 && page.MainLeft.Count == 0)
                {
                    page = ForceSingleLinePage(currentLine, pageNumber);
                }
                
                // שלב 3: כותרת רצה
                SetRunningHeader(page, currentLine);
                
                pages.Add(page);
                currentLine += page.MainRight.Count + page.MainLeft.Count;
                pageNumber++;
            }
            
            return pages;
        }
        
        /// <summary>
        /// שלב 1: ריווח כל הטקסט לרוחב הטור
        /// </summary>
        private void PreWrapAllText()
        {
            double columnWidth = LayoutConstants.COLUMN_WIDTH;
            
            // טקסט ראשי
            _mainWrapped = _textParser.WrapLinesToWidth(
                _doc.MainLines, 
                columnWidth, 
                LayoutConstants.FONT_SIZE_MAIN, 
                LayoutConstants.FONT_MAIN
            );
            
            // הערות A
            _footnotesA_Wrapped = _textParser.WrapLinesToWidth(
                _doc.FootnotesA,
                columnWidth,
                LayoutConstants.FONT_SIZE_FOOTNOTE_A,
                LayoutConstants.FONT_MAIN
            );
            
            // הערות B
            _footnotesB_Wrapped = _textParser.WrapLinesToWidth(
                _doc.FootnotesB,
                columnWidth,
                LayoutConstants.FONT_SIZE_FOOTNOTE_B,
                LayoutConstants.FONT_MAIN
            );
            
            // הערות C
            _footnotesC_Wrapped = _textParser.WrapLinesToWidth(
                _doc.FootnotesC,
                columnWidth,
                LayoutConstants.FONT_SIZE_FOOTNOTE_C,
                LayoutConstants.FONT_MAIN
            );
        }
        
        /// <summary>
        /// מנסה ליצור עמוד עם כמה שיותר שורות שמתאימות
        /// Greedy Fit Algorithm — לפי הדרכת קלוד
        /// </summary>
        private PageModel TryFitPage(int startLine, int pageNumber)
        {
            var page = new PageModel { 
                PageNumber = pageNumber,
                PageWidth = LayoutConstants.PAGE_WIDTH_PX,
                PageHeight = LayoutConstants.PAGE_HEIGHT_PX,
                ColumnWidth = LayoutConstants.COLUMN_WIDTH,
                ColumnGap = LayoutConstants.COLUMN_GAP,
                MarginOuter = LayoutConstants.MARGIN_OUTER,
                MarginInner = LayoutConstants.MARGIN_INNER
            };
            
            // שלב a: זיהוי הערות הרלוונטיות לטווח זה
            var footnotesA = GetFootnotesForRange(startLine, _doc.FootnotesA, LayoutConstants.REF_PREFIX_A);
            var footnotesB = GetFootnotesForRange(startLine, _doc.FootnotesB, LayoutConstants.REF_PREFIX_B);
            var footnotesC = GetFootnotesForRange(startLine, _doc.FootnotesC, LayoutConstants.REF_PREFIX_C);
            
            // שלב b: חישוב גובה הערות
            double fnA_Height = _heightCalc.MeasureFootnoteSectionHeight(
                footnotesA, LayoutConstants.FONT_SIZE_FOOTNOTE_A, LayoutConstants.COLUMN_WIDTH);
            double fnB_Height = _heightCalc.MeasureFootnoteSectionHeight(
                footnotesB, LayoutConstants.FONT_SIZE_FOOTNOTE_B, LayoutConstants.COLUMN_WIDTH);
            double fnC_Height = _heightCalc.MeasureFootnoteSectionHeight(
                footnotesC, LayoutConstants.FONT_SIZE_FOOTNOTE_C, LayoutConstants.COLUMN_WIDTH);
            
            // עדכון הגבהים ב-page
            page.FootnoteA_Height = fnA_Height;
            page.FootnoteB_Height = fnB_Height;
            page.FootnoteC_Height = fnC_Height;
            
            // שלב c: חישוב גובה זמין לטקסט ראשי
            double availableMainHeight = _heightCalc.AvailableMainHeight(page);
            
            // שלב d: Greedy Fit — התאמה חמדנית
            int maxLines = _heightCalc.HowManyLinesFit(
                availableMainHeight, 
                LayoutConstants.FONT_SIZE_MAIN, 
                LayoutConstants.LINE_HEIGHT_MAIN
            );
            
            // בדיקה: האם שורות + הערות נכנסים?
            int bestFit = 0;
            for (int tryLines = maxLines; tryLines >= 1; tryLines--)
            {
                var tryLinesList = _mainWrapped.Skip(startLine).Take(tryLines).ToList();
                double mainHeight = _heightCalc.CalculateMainHeight(tryLinesList);
                double totalHeight = mainHeight + fnA_Height + fnB_Height + fnC_Height;
                
                // הוספת גובה כותרות וקווי הפרדה
                totalHeight += LayoutConstants.RUNNING_HEADER_HEIGHT;
                if (fnA_Height > 0) totalHeight += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT + 2;
                if (fnB_Height > 0) totalHeight += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT + 2;
                if (fnC_Height > 0) totalHeight += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT + 2;
                
                if (totalHeight <= LayoutConstants.PAGE_HEIGHT_PX - LayoutConstants.MARGIN_TOP - LayoutConstants.MARGIN_BOTTOM)
                {
                    bestFit = tryLines;
                    break;
                }
            }
            
            // אם לא נמצאה התאמה — לפחות שורה אחת
            if (bestFit == 0) bestFit = 1;
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
        private PageModel ForceSingleLinePage(int lineIndex, int pageNumber)
        {
            var page = new PageModel 
            { 
                PageNumber = pageNumber,
                PageWidth = LayoutConstants.PAGE_WIDTH_PX,
                PageHeight = LayoutConstants.PAGE_HEIGHT_PX,
                ColumnWidth = LayoutConstants.COLUMN_WIDTH,
                ColumnGap = LayoutConstants.COLUMN_GAP,
                MarginOuter = LayoutConstants.MARGIN_OUTER,
                MarginInner = LayoutConstants.MARGIN_INNER
            };
            
            // שורה אחת בטור ימין
            if (lineIndex < _mainWrapped.Count)
            {
                page.MainRight = new List<string> { _mainWrapped[lineIndex] };
            }
            
            return page;
        }
        
        /// <summary>
        /// קובע כותרת רצה לעמוד
        /// </summary>
        private void SetRunningHeader(PageModel page, int startLine)
        {
            // ברירת מחדל — שם הספר והפרשה
            page.RunningHeaderRight = $"{_doc.BookName}";
            page.RunningHeaderCenter = _doc.ParashaName;
            page.RunningHeaderLeft = $"עמוד {page.PageNumber}";
            
            // אם יש מספיק שורות — עדכן לפי תוכן
            if (startLine < _mainWrapped.Count)
            {
                // כאן אפשר להוסיף לוגיקה מתקדמת לזיהוי פרק/פסוק
                // כרגע משתמשים בשם הפרשה כמרכז
            }
        }
        
        /// <summary>
        /// חלק את התוכן לשני טורים (ימין ושמאל) — עודכן למבנה החדש
        /// </summary>
        private void DistributeToColumns(PageModel page, List<string> allLines)
        {
            int totalLines = allLines.Count;
            if (totalLines == 0) return;
            
            // חלק שווה: חצי לימין, חצי לשמאל
            int rightCount = (int)Math.Ceiling(totalLines / 2.0);
            int leftCount = totalLines - rightCount;
            
            page.MainRight = allLines.Take(rightCount).ToList();
            page.MainLeft = allLines.Skip(rightCount).Take(leftCount).ToList();
        }
        
        /// <summary>
        /// מחלק רשימת הערות לשני טורים — עודכן למבנה החדש
        /// </summary>
        private void DistributeFootnotesToPage(PageModel page, List<string> footnotesA, List<string> footnotesB, List<string> footnotesC)
        {
            // הערות A
            int fnA_RightCount = (footnotesA.Count + 1) / 2;
            page.FootnotesA_Right = footnotesA.Take(fnA_RightCount).ToList();
            page.FootnotesA_Left = footnotesA.Skip(fnA_RightCount).ToList();
            
            // הערות B
            int fnB_RightCount = (footnotesB.Count + 1) / 2;
            page.FootnotesB_Right = footnotesB.Take(fnB_RightCount).ToList();
            page.FootnotesB_Left = footnotesB.Skip(fnB_RightCount).ToList();
            
            // הערות C
            int fnC_RightCount = (footnotesC.Count + 1) / 2;
            page.FootnotesC_Right = footnotesC.Take(fnC_RightCount).ToList();
            page.FootnotesC_Left = footnotesC.Skip(fnC_RightCount).ToList();
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
        /// מודד גובה הערות בפורמט run-on — מדידה מדויקת באמצעות FormattedText
        /// </summary>
        private double MeasureFootnoteHeight(int footnoteCount, double columnWidth, List<string> footnotes = null)
        {
            if (footnoteCount == 0) return 0;
            
            // אם יש לנו את הטקסט האמיתי, נמדוד אותו
            if (footnotes != null && footnotes.Count > 0)
            {
                return MeasureRunOnFootnotesHeight(footnotes, columnWidth, 11);
            }
            
            // הערכה חלופית: 2 הערות = שורה אחת, מחולק לשני טורים
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
        /// מוצא הערה לפי אינדקס — מחפש בכל מערכות ההערות
        /// </summary>
        private string GetFootnoteByIndex(ParsedDocument doc, int index)
        {
            // חפש בהערות מערכת 1
            if (index < doc.AllFootnotesA.Count)
                return doc.AllFootnotesA[index];
            
            // התאם את האינדקס להערות מערכת 2
            int adjustedIndex = index - doc.AllFootnotesA.Count;
            if (adjustedIndex >= 0 && adjustedIndex < doc.AllFootnotesB.Count)
                return doc.AllFootnotesB[adjustedIndex];
            
            // התאם את האינדקס להערות מערכת 3
            adjustedIndex = index - doc.AllFootnotesA.Count - doc.AllFootnotesB.Count;
            if (adjustedIndex >= 0 && adjustedIndex < doc.AllFootnotesC.Count)
                return doc.AllFootnotesC[adjustedIndex];
            
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
