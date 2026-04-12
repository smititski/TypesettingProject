using System;
using System.Globalization;
using System.Windows.Data;
using UI_Application.Models;

namespace UI_Application.Converters
{
    /// <summary>
    /// ממיר PageModel למחרוזת תיאור לתצוגה
    /// </summary>
    public class PageInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PageModel page)
            {
                int mainChars = page.MainContent?.Length ?? 0;
                int footnoteACount = page.FootnoteA?.Count ?? 0;
                int footnoteBCount = page.FootnoteB?.Count ?? 0;
                
                return $"עמוד {page.PageNumber}: {mainChars} תווים | הערות א': {footnoteACount} | הערות ב': {footnoteBCount}";
            }
            
            return "לא נבחר עמוד";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // לא נדרש להמרה חזרה
            throw new NotImplementedException();
        }
    }
}
