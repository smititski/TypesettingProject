using System.Collections.Generic;

namespace UI_Application.Models
{
    /// <summary>
    /// מודל נתונים לעמוד עימוד אחד עם תמיכה בהערות שוליים
    /// </summary>
    public class PageModel
    {
        /// <summary>
        /// מספר העמוד
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// התוכן הראשי שיישלח למנוע העימוד (C++)
        /// </summary>
        public string MainContent { get; set; } = string.Empty;

        /// <summary>
        /// רשימת הערות שוליים סוג א' (פירוש ראשון)
        /// </summary>
        public List<string> FootnoteA { get; set; } = new List<string>();

        /// <summary>
        /// רשימת הערות שוליים סוג ב' (פירוש שני)
        /// </summary>
        public List<string> FootnoteB { get; set; } = new List<string>();

        /// <summary>
        /// רשימת הערות שוליים סוג ג' (מדרש/הערות נוספות)
        /// </summary>
        public List<string> FootnoteC { get; set; } = new List<string>();

        /// <summary>
        /// גובה נותר בעמוד (בפיקסלים) - משמש לחישוב מתי לחתוך עמוד
        /// </summary>
        public double RemainingHeight { get; set; } = 600; // ברירת מחדל: גובה עמוד סטנדרטי

        /// <summary>
        /// רוחב העמוד בפיקסלים
        /// </summary>
        public double PageWidth { get; set; } = 800;

        /// <summary>
        /// גובה העמוד בפיקסלים
        /// </summary>
        public double PageHeight { get; set; } = 600;

        /// <summary>
        /// האם העמוד מלא
        /// </summary>
        public bool IsFull => RemainingHeight <= 0;

        /// <summary>
        /// מחשב את הגובה הכולל שנדרש לתוכן הראשי
        /// </summary>
        public double CalculateContentHeight(double lineHeight = 24)
        {
            if (string.IsNullOrEmpty(MainContent))
                return 0;

            // הערכה פשוטה: מספר שורות * גובה שורה
            var lines = MainContent.Split('\n');
            return lines.Length * lineHeight;
        }

        /// <summary>
        /// מחסיר גובה מהנותר ומחזיר אמת אם נשאר מקום
        /// </summary>
        public bool ReserveHeight(double height)
        {
            if (RemainingHeight >= height)
            {
                RemainingHeight -= height;
                return true;
            }
            return false;
        }

        /// <summary>
        /// מוסיף הערת שוליים סוג א'
        /// </summary>
        public void AddFootnoteA(string note)
        {
            FootnoteA.Add(note);
        }

        /// <summary>
        /// מוסיף הערת שוליים סוג ב'
        /// </summary>
        public void AddFootnoteB(string note)
        {
            FootnoteB.Add(note);
        }

        /// <summary>
        /// מוסיף הערת שוליים סוג ג'
        /// </summary>
        public void AddFootnoteC(string note)
        {
            FootnoteC.Add(note);
        }

        /// <summary>
        /// מחזיר סיכום של העמוד לצורך דיבוג
        /// </summary>
        public override string ToString()
        {
            return $"עמוד {PageNumber}: {MainContent?.Length ?? 0} תווים, " +
                   $"הערות א': {FootnoteA.Count}, הערות ב': {FootnoteB.Count}, הערות ג': {FootnoteC.Count}, " +
                   $"גובה נותר: {RemainingHeight:F0}px";
        }
    }
}
