using System.Collections.Generic;
using System.IO;
using UI_Application.Models;
using UI_Application.Constants;

namespace UI_Application.Services
{
    /// <summary>
    /// ייצוא PDF - שלב 7 לפי הדרכת קלוד
    /// </summary>
    public class PdfExporter
    {
        private readonly TextParser _textParser;
        
        public PdfExporter(TextParser textParser)
        {
            _textParser = textParser;
        }
        
        /// <summary>
        /// ייצוא רשימת עמודים לקובץ PDF
        /// </summary>
        public void ExportToPdf(List<PageModel> pages, string outputPath)
        {
            // PdfSharp 6.x - יש להתקין NuGet package
            // PdfSharp is not currently installed - placeholder implementation
            
            // Create placeholder file
            File.WriteAllText(outputPath + ".txt", 
                $"PDF Export Placeholder\nPages: {pages.Count}\n\n" +
                "To enable PDF export, install PdfSharp 6.x NuGet package:\n" +
                "dotnet add package PdfSharp");
        }
    }
}
