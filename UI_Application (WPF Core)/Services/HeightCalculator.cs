using System.Collections.Generic;
using UI_Application.Constants;
using UI_Application.Models;

namespace UI_Application.Services
{
    /// <summary>
    /// מנוע מדידת גובה — חישובי מידות לעימוד מדויק
    /// </summary>
    public class HeightCalculator
    {
        private readonly TextParser _textParser;
        
        public HeightCalculator(TextParser textParser)
        {
            _textParser = textParser;
        }
        
        /// <summary>
        /// חישוב גובה כולל של בלוק שורות
        /// </summary>
        public double MeasureBlockHeight(List<string> lines, double fontSize, double lineHeightMultiplier)
        {
            if (lines == null || lines.Count == 0) return 0;
            
            double lineHeightPx = fontSize * (96.0 / 72.0) * lineHeightMultiplier;
            return lines.Count * lineHeightPx;
        }
        
        /// <summary>
        /// חישוב גובה מדור הערות — כולל כותרת וקו מפריד
        /// </summary>
        public double MeasureFootnoteSectionHeight(List<string> footnotes, double fontSize, double columnWidth)
        {
            if (footnotes == null || footnotes.Count == 0) return 0;
            
            // ריווח הערות לרוחב הטור
            var wrapped = _textParser.WrapLinesToWidth(footnotes, columnWidth, fontSize, LayoutConstants.FONT_MAIN);
            
            // גובה התוכן
            double contentHeight = MeasureBlockHeight(wrapped, fontSize, LayoutConstants.LINE_HEIGHT_FOOTNOTE);
            
            // גובה כותרת המדור
            double titleHeight = LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT;
            
            // גובה קו המפריד
            double separatorHeight = 2; // px
            
            return contentHeight + titleHeight + separatorHeight;
        }
        
        /// <summary>
        /// כמה שורות נכנסות בגובה זמין
        /// </summary>
        public int HowManyLinesFit(double availableHeight, double fontSize, double lineHeightMultiplier)
        {
            if (availableHeight <= 0) return 0;
            
            double lineHeightPx = fontSize * (96.0 / 72.0) * lineHeightMultiplier;
            int lines = (int)(availableHeight / lineHeightPx);
            
            return lines > 0 ? lines : 0; // floor, never ceil
        }
        
        /// <summary>
        /// חישוב הגובה הפנוי לטקסט ראשי (לאחר הערות וכותרות)
        /// </summary>
        public double AvailableMainHeight(PageModel page)
        {
            // גובה בסיסי
            double baseHeight = LayoutConstants.PAGE_HEIGHT_PX 
                              - LayoutConstants.MARGIN_TOP 
                              - LayoutConstants.MARGIN_BOTTOM;
            
            // הפרשות גובה
            double deductions = 0;
            
            // כותרת רצה
            deductions += LayoutConstants.RUNNING_HEADER_HEIGHT;
            
            // מספר עמוד
            deductions += LayoutConstants.RUNNING_HEADER_OFFSET - LayoutConstants.PAGE_NUMBER_OFFSET;
            
            // הערות א' — אם יש
            if (page.FootnotesA_Right?.Count > 0 || page.FootnotesA_Left?.Count > 0)
            {
                deductions += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT;
                deductions += 2; // קו מפריד
                deductions += page.FootnoteA_Height;
            }
            
            // הערות ב' — אם יש
            if (page.FootnotesB_Right?.Count > 0 || page.FootnotesB_Left?.Count > 0)
            {
                deductions += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT;
                deductions += 2; // קו מפריד
                deductions += page.FootnoteB_Height;
            }
            
            // הערות ג' — אם יש
            if (page.FootnotesC_Right?.Count > 0 || page.FootnotesC_Left?.Count > 0)
            {
                deductions += LayoutConstants.FOOTNOTE_SECTION_TITLE_HEIGHT;
                deductions += 2; // קו מפריד
                deductions += page.FootnoteC_Height;
            }
            
            // מרווח בין הערות לטקסט ראשי
            deductions += 10;
            
            return baseHeight - deductions;
        }
        
        /// <summary>
        /// חישוב גובה טקסט ראשי בלבד
        /// </summary>
        public double CalculateMainHeight(List<string> mainLines)
        {
            return MeasureBlockHeight(
                mainLines, 
                LayoutConstants.FONT_SIZE_MAIN, 
                LayoutConstants.LINE_HEIGHT_MAIN
            );
        }
        
        /// <summary>
        /// בדיקה האם עמוד מלא
        /// </summary>
        public bool IsPageFull(PageModel page, double additionalHeight)
        {
            double available = AvailableMainHeight(page);
            double used = page.MainHeightUsed + additionalHeight;
            return used > available;
        }
    }
}
