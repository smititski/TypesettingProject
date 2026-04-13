using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UI_Application.ViewModels;
using UI_Application.Services;

namespace UI_Application.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Subscribe to DataContext changes to handle glyph rendering
        DataContextChanged += OnDataContextChanged;
        
        // If DataContext already set, subscribe directly
        if (DataContext is EditorViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is EditorViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is EditorViewModel newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.Glyphs) || e.PropertyName == nameof(EditorViewModel.CurrentPage))
        {
            var vm = (EditorViewModel)DataContext;
            DrawGlyphs(vm.Glyphs, vm.CurrentPage?.FootnoteA, vm.CurrentPage?.FootnoteB, vm.CurrentPage?.FootnoteC, 
                vm.CurrentPage?.ChapterTitle ?? vm.ChapterTitle, vm.CurrentPage?.PageNumber ?? 1);
        }
    }

    /// <summary>
    /// Draw glyphs on the canvas with footnotes, justify alignment, and Torah spacing.
    /// </summary>
    public void DrawGlyphs(IEnumerable<GlyphInfo> glyphs, List<string> footnotesA = null, List<string> footnotesB = null, List<string> footnotesC = null, string chapterTitle = "", int pageNumber = 1)
    {
        // Clear existing children
        GlyphCanvas.Children.Clear();

        if (glyphs == null)
            return;

        var glyphList = glyphs.ToList();
        if (glyphList.Count == 0)
            return;

        double fontSize = 24;
        double lineHeight = fontSize * 1.7; // מרווח תורני - 1.7 מגודל הפונט
        double startY = 80; // שמירת מקום לכותרת עליונה
        double currentY = startY;
        double pageWidth = 500;
        double rightMargin = 30;
        double leftMargin = 30;

        // ציור כותרת עליונה עם שם הפרק
        if (!string.IsNullOrEmpty(chapterTitle))
        {
            var headerBlock = new System.Windows.Controls.TextBlock
            {
                Text = chapterTitle,
                FontSize = 14,
                FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
                Foreground = System.Windows.Media.Brushes.DarkGray,
                FlowDirection = System.Windows.FlowDirection.RightToLeft,
                FontWeight = System.Windows.FontWeights.Bold
            };
            Canvas.SetRight(headerBlock, rightMargin);
            Canvas.SetTop(headerBlock, 20);
            GlyphCanvas.Children.Add(headerBlock);
        }

        // ציור מספר עמוד בפינה הימנית-תחתונה - ימוקם בסוף העמוד
        var pageNumberBlock = new System.Windows.Controls.TextBlock
        {
            Text = pageNumber.ToString(),
            FontSize = 16,
            FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
            Foreground = System.Windows.Media.Brushes.Black,
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontWeight = System.Windows.FontWeights.Bold
        };
        // מיקום בתחתית העמוד - יעודכן בסוף הפונקציה לפי הגובה האמיתי
        GlyphCanvas.Children.Add(pageNumberBlock);
        // שמירת התייחסות לעדכון מיקום מאוחר יותר
        System.Windows.Controls.TextBlock savedPageNumberBlock = pageNumberBlock;

        // קיבוץ גליפים לפי שורות (לפי ערך Y)
        var lines = glyphList
            .GroupBy(g => Math.Round(g.Y / 10.0) * 10)
            .OrderBy(g => g.Key)
            .ToList();

        // רינדור הטקסט הראשי עם Justify
        // קודם כל נחשב את כל השורות
        var allLines = new List<string>();
        foreach (var lineGroup in lines)
        {
            var lineText = new string(lineGroup.OrderBy(g => g.X).Select(g => (char)g.Id).ToArray());
            var wrappedLines = TextHelper.WrapText(lineText, maxCharsPerLine: 40);
            allLines.AddRange(wrappedLines);
        }
        
        int totalLines = allLines.Count;
        int lineIndex = 0;
        
        foreach (var wrappedLine in allLines)
        {
            lineIndex++;
            bool isLastLine = (lineIndex == totalLines);
            
            var words = wrappedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length > 1 && !isLastLine)
            {
                // Justify - חישוב רווחים ליישור לשני הצדדים
                double totalWordsWidth = 0;
                foreach (var word in words)
                    totalWordsWidth += MeasureTextWidth(word, fontSize);
                
                double availableWidth = pageWidth - rightMargin - leftMargin;
                double extraSpace = availableWidth - totalWordsWidth;
                double spaceBetweenWords = extraSpace / (words.Length - 1);
                
                // רינדור מהימין לשמאל
                double currentX = rightMargin;
                
                for (int i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    var wordBlock = new System.Windows.Controls.TextBlock
                    {
                        Text = word,
                        FontSize = fontSize,
                        FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
                        Foreground = System.Windows.Media.Brushes.Black,
                        FlowDirection = System.Windows.FlowDirection.RightToLeft
                    };
                    
                    Canvas.SetRight(wordBlock, currentX);
                    Canvas.SetTop(wordBlock, currentY);
                    GlyphCanvas.Children.Add(wordBlock);
                    
                    currentX += MeasureTextWidth(word, fontSize) + spaceBetweenWords;
                }
            }
            else
            {
                // שורה אחרונה או מילה אחת - יישור ימין
                var textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = wrappedLine,
                    FontSize = fontSize,
                    FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
                    Foreground = System.Windows.Media.Brushes.Black,
                    FlowDirection = System.Windows.FlowDirection.RightToLeft
                };
                Canvas.SetRight(textBlock, rightMargin);
                Canvas.SetTop(textBlock, currentY);
                GlyphCanvas.Children.Add(textBlock);
            }
            
            currentY += lineHeight;
        }
        
        // הוספת מרווח לפני מדורי הערות
        currentY += lineHeight * 0.5;
        
        // ציור קו מפריד ארוך בין טקסט ראשי למדור א'
        DrawSeparatorLine(currentY, leftMargin, pageWidth - rightMargin, true);
        currentY += 25;
        
        // רינדור מדור א' - ביאור
        currentY = RenderFootnoteSection(footnotesA, currentY, lineHeight, fontSize * 0.85, 
            "ביאור א'", System.Windows.Media.Brushes.DarkBlue, leftMargin, pageWidth - rightMargin);
        
        // ציור קו מפריד קצר במרכז בין א' לב'
        if (footnotesA?.Count > 0 && footnotesB?.Count > 0)
        {
            currentY += 10;
            double centerX = (leftMargin + pageWidth - rightMargin) / 2;
            DrawSeparatorLine(currentY, centerX - 100, centerX + 100, false);
            currentY += 20;
        }
        
        // רינדור מדור ב' - מקורות
        currentY = RenderFootnoteSection(footnotesB, currentY, lineHeight, fontSize * 0.85, 
            "מקורות", System.Windows.Media.Brushes.DarkGreen, leftMargin, pageWidth - rightMargin);
        
        // ציור קו מפריד קצר במרכז בין ב' לג'
        if (footnotesB?.Count > 0 && footnotesC?.Count > 0)
        {
            currentY += 10;
            double centerX = (leftMargin + pageWidth - rightMargin) / 2;
            DrawSeparatorLine(currentY, centerX - 100, centerX + 100, false);
            currentY += 20;
        }
        
        // רינדור מדור ג' - הערות נוספות
        currentY = RenderFootnoteSection(footnotesC, currentY, lineHeight, fontSize * 0.85, 
            "הערות נוספות", System.Windows.Media.Brushes.DarkOrange, leftMargin, pageWidth - rightMargin);
        
        // עדכון מיקום מספר העמוד בתחתית העמוד האמיתית
        double finalHeight = Math.Max(700, currentY + 50);
        Canvas.SetRight(savedPageNumberBlock, rightMargin);
        Canvas.SetTop(savedPageNumberBlock, finalHeight - 40);
        
        // Force layout update
        GlyphCanvas.InvalidateVisual();

        // Update canvas size
        GlyphCanvas.Height = finalHeight;
        GlyphCanvas.Width = pageWidth + leftMargin + rightMargin;
    }
    
    /// <summary>
    /// מודד רוחב טקסט בפיקסלים
    /// </summary>
    private double MeasureTextWidth(string text, double fontSize)
    {
        var formattedText = new System.Windows.Media.FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.RightToLeft,
            new System.Windows.Media.Typeface("Frank Ruehl"),
            fontSize,
            System.Windows.Media.Brushes.Black,
            1.0);
        return formattedText.Width;
    }
    
    /// <summary>
    /// מצייר קו מפריד אופקי
    /// </summary>
    private void DrawSeparatorLine(double y, double x1, double x2, bool isLong)
    {
        var line = new System.Windows.Shapes.Line
        {
            X1 = x1,
            Y1 = y,
            X2 = x2,
            Y2 = y,
            Stroke = System.Windows.Media.Brushes.Gray,
            StrokeThickness = isLong ? 1.5 : 1,
            StrokeDashArray = isLong ? null : new DoubleCollection { 3, 3 }
        };
        GlyphCanvas.Children.Add(line);
    }
    
    /// <summary>
    /// מרנדר מדור הערות שלם עם כותרת ו-Justify
    /// </summary>
    private double RenderFootnoteSection(List<string> footnotes, double startY, double lineHeight, 
        double fontSize, string sectionTitle, System.Windows.Media.Brush color, double leftMargin, double rightMargin)
    {
        if (footnotes == null || footnotes.Count == 0)
            return startY;
        
        double currentY = startY;
        double pageWidth = 500;
        double availableWidth = pageWidth - leftMargin - rightMargin;
        
        // כותרת המדור
        var titleBlock = new System.Windows.Controls.TextBlock
        {
            Text = sectionTitle,
            FontSize = fontSize * 1.1,
            FontWeight = System.Windows.FontWeights.Bold,
            FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
            Foreground = color,
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        
        // מרכוז הכותרת
        titleBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        double titleWidth = titleBlock.DesiredSize.Width;
        double centerX = (pageWidth - titleWidth) / 2;
        
        Canvas.SetRight(titleBlock, centerX);
        Canvas.SetTop(titleBlock, currentY);
        GlyphCanvas.Children.Add(titleBlock);
        currentY += fontSize * 1.5;
        
        // רינדור כל הערה במדור עם Justify
        for (int i = 0; i < footnotes.Count; i++)
        {
            currentY = RenderJustifiedNote(footnotes[i], currentY, fontSize, color, 
                availableWidth, rightMargin, i + 1);
            currentY += 12; // מרווח בין הערות
        }
        
        return currentY;
    }
    
    /// <summary>
    /// מרנדר הערה בודדת עם Justify ומילה ראשונה מודגשת
    /// </summary>
    private double RenderJustifiedNote(string noteText, double startY, double fontSize, 
        System.Windows.Media.Brush color, double availableWidth, double rightMargin, int noteNumber)
    {
        // חילוץ המילה הראשונה (עד הנקודתיים או הרווח הראשון)
        string prefix = "";
        string rest = noteText;
        
        int colonIndex = noteText.IndexOf(':');
        int spaceIndex = noteText.IndexOf(' ');
        
        if (colonIndex > 0 && colonIndex < 30)
        {
            prefix = noteText.Substring(0, colonIndex + 1);
            rest = noteText.Substring(colonIndex + 1).Trim();
        }
        else if (spaceIndex > 0 && spaceIndex < 20)
        {
            prefix = noteText.Substring(0, spaceIndex);
            rest = noteText.Substring(spaceIndex).Trim();
        }
        
        // יצירת StackPanel לסידור אופקי
        var panel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            FlowDirection = System.Windows.FlowDirection.RightToLeft
        };
        
        double currentX = rightMargin;
        
        // הוספת מספר ההערה בתחילה
        var numberBlock = new System.Windows.Controls.TextBlock
        {
            Text = noteNumber + ". ",
            FontSize = fontSize,
            FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
            Foreground = color,
            FontWeight = System.Windows.FontWeights.Bold,
            Margin = new System.Windows.Thickness(4, 0, 0, 0)
        };
        panel.Children.Add(numberBlock);
        
        // אם יש מילה ראשונה מיוחדת - מודגשת
        if (!string.IsNullOrEmpty(prefix))
        {
            var prefixBlock = new System.Windows.Controls.TextBlock
            {
                Text = prefix,
                FontSize = fontSize * 1.1,
                FontWeight = System.Windows.FontWeights.Bold,
                FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
                Foreground = color,
                Margin = new System.Windows.Thickness(0, 0, 4, 0)
            };
            panel.Children.Add(prefixBlock);
        }
        
        // שאר הטקסט עם Justify
        if (!string.IsNullOrEmpty(rest))
        {
            var words = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                // חישוב Justify
                double totalWordsWidth = 0;
                foreach (var word in words)
                    totalWordsWidth += MeasureTextWidth(word, fontSize);
                
                double extraSpace = availableWidth - totalWordsWidth - (string.IsNullOrEmpty(prefix) ? 0 : MeasureTextWidth(prefix, fontSize * 1.1));
                double spaceBetweenWords = extraSpace / (words.Length - 1);
                
                for (int i = 0; i < words.Length; i++)
                {
                    var wordBlock = new System.Windows.Controls.TextBlock
                    {
                        Text = words[i],
                        FontSize = fontSize,
                        FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
                        Foreground = color
                    };
                    
                    if (i < words.Length - 1)
                        wordBlock.Margin = new System.Windows.Thickness(spaceBetweenWords, 0, 0, 0);
                    
                    panel.Children.Add(wordBlock);
                }
            }
            else
            {
                // מילה אחת
                var textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = rest,
                    FontSize = fontSize,
                    FontFamily = new System.Windows.Media.FontFamily("Frank Ruehl"),
                    Foreground = color
                };
                panel.Children.Add(textBlock);
            }
        }
        
        // מדידת הגובה
        panel.Measure(new System.Windows.Size(availableWidth, double.PositiveInfinity));
        
        Canvas.SetRight(panel, rightMargin);
        Canvas.SetTop(panel, startY);
        GlyphCanvas.Children.Add(panel);
        
        return startY + panel.DesiredSize.Height;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Clean up event handlers
        if (DataContext is EditorViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
        DataContextChanged -= OnDataContextChanged;

        base.OnClosing(e);
    }
}

/// <summary>
/// Custom UI element that renders glyphs using DrawingContext.
/// </summary>
public class GlyphRenderer : UIElement
{
    private readonly List<GlyphInfo> _glyphs;
    private readonly Brush _glyphBrush;
    private readonly Brush _textBrush;
    private readonly Pen _outlinePen;

    public GlyphRenderer(List<GlyphInfo> glyphs)
    {
        _glyphs = glyphs;
        _glyphBrush = new SolidColorBrush(Color.FromRgb(66, 133, 244));
        _textBrush = Brushes.White;
        _outlinePen = new Pen(Brushes.DarkBlue, 1);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        foreach (var glyph in _glyphs)
        {
            // Draw glyph placeholder rectangle at (X, Y)
            var rect = new Rect(glyph.X, glyph.Y, 16, 20);
            drawingContext.DrawRectangle(_glyphBrush, _outlinePen, rect);

            // Draw the actual character next to the rectangle using FormattedText
            var formattedText = new FormattedText(
                ((char)glyph.Id).ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.RightToLeft,
                new Typeface("Segoe UI"),
                12,
                _textBrush,
                1.0);

            // Position text centered in the rectangle
            drawingContext.DrawText(
                formattedText,
                new Point(glyph.X + 2, glyph.Y + 2));

            // Draw glyph ID as small label below
            var idText = new FormattedText(
                $"{glyph.Id}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.RightToLeft,
                new Typeface("Consolas"),
                8,
                Brushes.Gray,
                1.0);

            drawingContext.DrawText(
                idText,
                new Point(glyph.X, glyph.Y + 22));
        }

        // Draw baseline indicator
        if (_glyphs.Count > 0)
        {
            var baselineY = _glyphs[0].Y + 20;
            var startX = _glyphs.Min(g => g.X) - 10;
            var endX = _glyphs.Max(g => g.X) + 30;
            
            drawingContext.DrawLine(
                new Pen(Brushes.Red, 1) { DashStyle = DashStyles.Dash },
                new Point(startX, baselineY),
                new Point(endX, baselineY));
        }
    }
}
