using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UI_Application.Constants;
using UI_Application.Models;
using UI_Application.Services;

namespace UI_Application.Rendering
{
    /// <summary>
    /// מנוע רינדור — מצייר עמוד דו-טורי על Canvas
    /// Traditional Scholarly Page Layout — 6 שכבות
    /// </summary>
    public class PageRenderer
    {
        private readonly TextParser _textParser;
        private Canvas? _canvas;
        private double _currentY;
        
        public PageRenderer(TextParser textParser)
        {
            _textParser = textParser;
        }
        
        /// <summary>
        /// הרינדור הראשי — 6 שכבות
        /// </summary>
        public void RenderPage(PageModel page, Canvas canvas)
        {
            _canvas = canvas;
            _canvas.Children.Clear();
            
            // מידות
            _canvas.Width = LayoutConstants.PAGE_WIDTH_PX;
            _canvas.Height = LayoutConstants.PAGE_HEIGHT_PX;
            
            // רקע לבן (שכבה 1)
            DrawBackground();
            
            // כותרת רצה + מספר עמוד (שכבות 2-3)
            _currentY = LayoutConstants.MARGIN_TOP - LayoutConstants.RUNNING_HEADER_OFFSET;
            DrawRunningHeader(page);
            DrawPageNumber(page);
            
            // טקסט ראשי — שני טורים (שכבה 4)
            _currentY = LayoutConstants.MARGIN_TOP;
            DrawMainText(page);
            
            // קווי הפרדה (שכבה 5)
            DrawSeparatorLines(page);
            
            // הערות שוליים (שכבה 6)
            DrawFootnotes(page);
        }
        
        /// <summary>
        /// שכבה 1 — רקע לבן
        /// </summary>
        private void DrawBackground()
        {
            var bg = new Rectangle
            {
                Width = LayoutConstants.PAGE_WIDTH_PX,
                Height = LayoutConstants.PAGE_HEIGHT_PX,
                Fill = Brushes.White
            };
            Canvas.SetLeft(bg, 0);
            Canvas.SetTop(bg, 0);
            _canvas?.Children.Add(bg);
        }
        
        /// <summary>
        /// שכבה 2 — כותרת רצה
        /// </summary>
        private void DrawRunningHeader(PageModel page)
        {
            if (string.IsNullOrEmpty(page.RunningHeaderCenter)) return;
            
            double y = LayoutConstants.MARGIN_TOP - LayoutConstants.RUNNING_HEADER_OFFSET;
            double leftX = LayoutConstants.MARGIN_OUTER;
            double rightX = LayoutConstants.PAGE_WIDTH_PX - LayoutConstants.MARGIN_OUTER;
            double centerX = LayoutConstants.PAGE_WIDTH_PX / 2;
            
            // ימין
            if (!string.IsNullOrEmpty(page.RunningHeaderRight))
            {
                DrawText(page.RunningHeaderRight, leftX, y, LayoutConstants.FONT_SIZE_RUNNING_HEADER, 
                    LayoutConstants.FONT_MAIN, Brushes.Gray, TextAlignment.Right);
            }
            
            // מרכז
            DrawText(page.RunningHeaderCenter, centerX, y, LayoutConstants.FONT_SIZE_RUNNING_HEADER,
                LayoutConstants.FONT_MAIN, Brushes.Gray, TextAlignment.Center);
            
            // שמאל
            if (!string.IsNullOrEmpty(page.RunningHeaderLeft))
            {
                DrawText(page.RunningHeaderLeft, rightX, y, LayoutConstants.FONT_SIZE_RUNNING_HEADER,
                    LayoutConstants.FONT_MAIN, Brushes.Gray, TextAlignment.Left);
            }
            
            // קו דק מתחת לכותרת
            var line = new Line
            {
                X1 = LayoutConstants.MARGIN_OUTER,
                Y1 = LayoutConstants.MARGIN_TOP - 4,
                X2 = LayoutConstants.PAGE_WIDTH_PX - LayoutConstants.MARGIN_OUTER,
                Y2 = LayoutConstants.MARGIN_TOP - 4,
                Stroke = Brushes.Gray,
                StrokeThickness = LayoutConstants.HEADER_LINE_THICKNESS
            };
            _canvas?.Children.Add(line);
        }
        
        /// <summary>
        /// שכבה 3 — מספר עמוד
        /// </summary>
        private void DrawPageNumber(PageModel page)
        {
            double y = LayoutConstants.MARGIN_TOP - LayoutConstants.PAGE_NUMBER_OFFSET;
            double centerX = LayoutConstants.PAGE_WIDTH_PX / 2;
            
            DrawText(page.PageNumber.ToString(), centerX, y, LayoutConstants.FONT_SIZE_PAGE_NUMBER,
                LayoutConstants.FONT_MAIN, Brushes.Black, TextAlignment.Center);
        }
        
        /// <summary>
        /// שכבה 4 — טקסט ראשי בשני טורים
        /// </summary>
        private void DrawMainText(PageModel page)
        {
            double y = LayoutConstants.MARGIN_TOP;
            double colWidth = LayoutConstants.COLUMN_WIDTH;
            double colGap = LayoutConstants.COLUMN_GAP;
            double rightX = LayoutConstants.MARGIN_INNER + colWidth + colGap; // טור ימין (RTL)
            double leftX = LayoutConstants.MARGIN_INNER; // טור שמאל (RTL)
            
            // רינדור טור ימין (הטור הראשון לקריאה)
            RenderColumn(page.MainRight, rightX, y, colWidth, 
                LayoutConstants.FONT_SIZE_MAIN, true);
            
            // רינדור טור שמאל (הטור השני לקריאה)
            RenderColumn(page.MainLeft, leftX, y, colWidth,
                LayoutConstants.FONT_SIZE_MAIN, false);
        }
        
        /// <summary>
        /// רינדור טור טקסט
        /// </summary>
        private void RenderColumn(List<string> lines, double x, double startY, 
            double width, double fontSize, bool isRightColumn)
        {
            if (lines == null || lines.Count == 0) return;
            
            double y = startY;
            double lineHeight = fontSize * (96.0 / 72.0) * LayoutConstants.LINE_HEIGHT_MAIN;
            
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                bool isLastLine = _textParser.IsLastLine(line);
                string cleanLine = _textParser.RemoveLastLineMarker(line);
                
                if (isLastLine)
                {
                    // שורה אחרונה — יישור ימין בלבד (לא מיושרת)
                    DrawRightAlignedLine(cleanLine, x + width, y, fontSize, width);
                }
                else
                {
                    // שורה רגילה — יישור דו-צדדי
                    RenderJustifiedLine(cleanLine, x, y, width, fontSize);
                }
                
                y += lineHeight;
            }
        }
        
        /// <summary>
        /// יישור דו-צדדי — חלוקת רווחים שווה בין מילים
        /// </summary>
        private void RenderJustifiedLine(string line, double x, double y, double width, double fontSize)
        {
            var words = line.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
            if (words.Count == 0) return;
            if (words.Count == 1)
            {
                // מילה אחת — יישור ימין
                DrawRightAlignedLine(words[0], x + width, y, fontSize, width);
                return;
            }
            
            // מדידת רוחב כל המילים (ללא ניקוד)
            double totalWordsWidth = 0;
            var wordWidths = new List<double>();
            foreach (var word in words)
            {
                double w = _textParser.MeasureTextWidth(word, fontSize, LayoutConstants.FONT_MAIN);
                wordWidths.Add(w);
                totalWordsWidth += w;
            }
            
            // חישוב רווח בין מילים
            double gapCount = words.Count - 1;
            double extraSpace = (width - totalWordsWidth) / gapCount;
            if (extraSpace < 0) extraSpace = 0; // אם הטקסט ארוך מדי
            
            // מיקום מילים מימין לשמאל (RTL)
            double currentX = x + width;
            for (int i = 0; i < words.Count; i++)
            {
                double wordWidth = wordWidths[i];
                currentX -= wordWidth;
                
                // ציור המילה (עם ניקוד!)
                DrawWord(words[i], currentX, y, fontSize);
                
                currentX -= extraSpace;
            }
        }
        
        /// <summary>
        /// יישור ימין לשורה בודדת
        /// </summary>
        private void DrawRightAlignedLine(string line, double rightX, double y, double fontSize, double maxWidth)
        {
            var words = line.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
            if (words.Count == 0) return;
            
            double currentX = rightX;
            for (int i = 0; i < words.Count; i++)
            {
                double wordWidth = _textParser.MeasureTextWidth(words[i], fontSize, LayoutConstants.FONT_MAIN);
                currentX -= wordWidth;
                DrawWord(words[i], currentX, y, fontSize);
                currentX -= 3; // רווח קטן בין מילים בשורה אחרונה
            }
        }
        
        /// <summary>
        /// ציור מילה בודדת (עם ניקוד!)
        /// </summary>
        private void DrawWord(string word, double x, double y, double fontSize)
        {
            DrawText(word, x, y, fontSize, LayoutConstants.FONT_MAIN, Brushes.Black, TextAlignment.Right);
        }
        
        /// <summary>
        /// שכבה 5 — קווי הפרדה
        /// </summary>
        private void DrawSeparatorLines(PageModel page)
        {
            double y = _currentY + 10; // אחרי הטקסט הראשי
            double colWidth = LayoutConstants.COLUMN_WIDTH;
            double colGap = LayoutConstants.COLUMN_GAP;
            double rightX = LayoutConstants.MARGIN_INNER + colWidth + colGap;
            double leftX = LayoutConstants.MARGIN_INNER;
            double lineWidth = colWidth * 0.4; // 40% מרוחב הטור
            
            // קו אחרי טקסט ראשי — אם יש הערות
            if (page.FootnotesA_Right.Count > 0 || page.FootnotesA_Left.Count > 0 ||
                page.FootnotesB_Right.Count > 0 || page.FootnotesB_Left.Count > 0 ||
                page.FootnotesC_Right.Count > 0 || page.FootnotesC_Left.Count > 0)
            {
                // טור ימין
                var lineRight = new Line
                {
                    X1 = rightX + colWidth - lineWidth,
                    Y1 = y,
                    X2 = rightX + colWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LayoutConstants.COLOR_SEPARATOR_LINE)),
                    StrokeThickness = LayoutConstants.SEPARATOR_LINE_THICKNESS
                };
                _canvas?.Children.Add(lineRight);
                
                // טור שמאל
                var lineLeft = new Line
                {
                    X1 = leftX + colWidth - lineWidth,
                    Y1 = y,
                    X2 = leftX + colWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LayoutConstants.COLOR_SEPARATOR_LINE)),
                    StrokeThickness = LayoutConstants.SEPARATOR_LINE_THICKNESS
                };
                _canvas?.Children.Add(lineLeft);
                
                _currentY = y + 5;
            }
        }
        
        /// <summary>
        /// שכבה 6 — הערות שוליים
        /// </summary>
        private void DrawFootnotes(PageModel page)
        {
            // מערכת א'
            if (page.FootnotesA_Right.Count > 0 || page.FootnotesA_Left.Count > 0)
            {
                RenderFootnoteSection("פירוש ראשון", page.FootnotesA_Right, page.FootnotesA_Left,
                    LayoutConstants.FONT_SIZE_FOOTNOTE_A, "א");
            }
            
            // מערכת ב'
            if (page.FootnotesB_Right.Count > 0 || page.FootnotesB_Left.Count > 0)
            {
                RenderFootnoteSection("פירוש שני", page.FootnotesB_Right, page.FootnotesB_Left,
                    LayoutConstants.FONT_SIZE_FOOTNOTE_B, "ב");
            }
            
            // מערכת ג'
            if (page.FootnotesC_Right.Count > 0 || page.FootnotesC_Left.Count > 0)
            {
                RenderFootnoteSection("מדרש", page.FootnotesC_Right, page.FootnotesC_Left,
                    LayoutConstants.FONT_SIZE_FOOTNOTE_C, "ג");
            }
        }
        
        /// <summary>
        /// רינדור מדור הערות שלם
        /// </summary>
        private void RenderFootnoteSection(string title, List<string> rightLines, List<string> leftLines,
            double fontSize, string sectionMarker)
        {
            double y = _currentY;
            double colWidth = LayoutConstants.COLUMN_WIDTH;
            double colGap = LayoutConstants.COLUMN_GAP;
            double rightX = LayoutConstants.MARGIN_INNER + colWidth + colGap;
            double leftX = LayoutConstants.MARGIN_INNER;
            
            // כותרת המדור — מודגשת, מרכזית
            var titleText = new TextBlock
            {
                Text = title,
                FontFamily = new System.Windows.Media.FontFamily(LayoutConstants.FONT_MAIN),
                FontSize = fontSize + 1,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Width = colWidth * 2 + colGap
            };
            Canvas.SetLeft(titleText, leftX);
            Canvas.SetTop(titleText, y);
            _canvas?.Children.Add(titleText);
            
            y += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT;
            
            // קו הפרדה
            double lineWidth = colWidth * 0.4;
            var sepLineRight = new Line
            {
                X1 = rightX + colWidth - lineWidth,
                Y1 = y - 8,
                X2 = rightX + colWidth,
                Y2 = y - 8,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LayoutConstants.COLOR_SEPARATOR_LINE)),
                StrokeThickness = LayoutConstants.SEPARATOR_LINE_THICKNESS
            };
            _canvas?.Children.Add(sepLineRight);
            
            var sepLineLeft = new Line
            {
                X1 = leftX + colWidth - lineWidth,
                Y1 = y - 8,
                X2 = leftX + colWidth,
                Y2 = y - 8,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LayoutConstants.COLOR_SEPARATOR_LINE)),
                StrokeThickness = LayoutConstants.SEPARATOR_LINE_THICKNESS
            };
            _canvas?.Children.Add(sepLineLeft);
            
            // רינדור הערות בשני טורים — פורמט run-on
            RenderRunOnColumn(rightLines, rightX, y, colWidth, fontSize, sectionMarker);
            RenderRunOnColumn(leftLines, leftX, y, colWidth, fontSize, sectionMarker);
            
            // עדכון המיקום לסעיף הבא
            double rightHeight = CalculateRunOnHeight(rightLines, colWidth, fontSize);
            double leftHeight = CalculateRunOnHeight(leftLines, colWidth, fontSize);
            _currentY = y + Math.Max(rightHeight, leftHeight) + 15;
        }
        
        /// <summary>
        /// רינדור טור הערות בפורמט run-on
        /// </summary>
        private void RenderRunOnColumn(List<string> notes, double x, double y, double width, 
            double fontSize, string sectionMarker)
        {
            if (notes.Count == 0) return;
            
            // בניית טקסט run-on עם מרקרים
            var sb = new StringBuilder();
            var hebrewLetters = new[] { "א", "ב", "ג", "ד", "ה", "ו", "ז", "ח", "ט", "י",
                "יא", "יב", "יג", "יד", "טו", "טז", "יז", "יח", "יט", "כ" };
            
            for (int i = 0; i < notes.Count; i++)
            {
                if (i > 0) sb.Append(" ");
                
                // בחירת מרקר לפי מערכת
                string marker = sectionMarker switch
                {
                    "א" => $"[{hebrewLetters[i]}]",
                    "ב" => $"[{i + 1}]",
                    "ג" => $"[{new[] { "*", "†", "‡", "§" }[i % 4]}]",
                    _ => $"[{i + 1}]"
                };
                
                sb.Append(marker);
                sb.Append(" ");
                sb.Append(notes[i]);
            }
            
            string runOnText = sb.ToString();
            
            // ריווח לרוחב הטור
            var wrappedLines = WrapTextToWidth(runOnText, width, fontSize);
            
            // רינדור השורות
            double currentY = y;
            double lineHeight = fontSize * (96.0 / 72.0) * LayoutConstants.LINE_HEIGHT_FOOTNOTE;
            
            foreach (var line in wrappedLines)
            {
                RenderJustifiedLine(line, x, currentY, width, fontSize);
                currentY += lineHeight;
            }
        }
        
        /// <summary>
        /// חישוב גובה טור run-on
        /// </summary>
        private double CalculateRunOnHeight(List<string> notes, double width, double fontSize)
        {
            if (notes.Count == 0) return 0;
            
            var sb = new StringBuilder();
            for (int i = 0; i < notes.Count; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append($"[{i + 1}] {notes[i]}");
            }
            
            var wrapped = WrapTextToWidth(sb.ToString(), width, fontSize);
            double lineHeight = fontSize * (96.0 / 72.0) * LayoutConstants.LINE_HEIGHT_FOOTNOTE;
            return wrapped.Count * lineHeight;
        }
        
        /// <summary>
        /// ריווח טקסט לרוחב מסוים
        /// </summary>
        private List<string> WrapTextToWidth(string text, double maxWidth, double fontSize)
        {
            var words = text.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
            var result = new List<string>();
            var currentLine = new StringBuilder();
            
            foreach (var word in words)
            {
                var testLine = currentLine.Length > 0 ? currentLine + " " + word : word;
                double width = _textParser.MeasureTextWidth(testLine, fontSize, LayoutConstants.FONT_MAIN);
                
                if (width <= maxWidth)
                {
                    if (currentLine.Length > 0) currentLine.Append(" ");
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        result.Add(currentLine.ToString());
                        currentLine.Clear();
                    }
                    currentLine.Append(word);
                }
            }
            
            if (currentLine.Length > 0)
            {
                result.Add(currentLine.ToString());
            }
            
            return result;
        }
        
        /// <summary>
        /// שיטת עזר לציור טקסט בסיסית
        /// </summary>
        private void DrawText(string text, double x, double y, double fontSize, 
            string fontFamily, Brush brush, TextAlignment alignment)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontFamily = new System.Windows.Media.FontFamily(fontFamily),
                FontSize = fontSize,
                Foreground = brush,
                TextAlignment = alignment,
                FlowDirection = FlowDirection.RightToLeft
            };
            
            // מדידת רוחב ליישור נכון
            var width = _textParser.MeasureTextWidth(text, fontSize, fontFamily);
            
            double left = alignment switch
            {
                TextAlignment.Right => x - width,
                TextAlignment.Center => x - width / 2,
                TextAlignment.Left => x,
                _ => x
            };
            
            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, y);
            _canvas?.Children.Add(textBlock);
        }
    }
}
