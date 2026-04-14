namespace UI_Application.Models;

/// <summary>
/// מסמך מנותח — מכיל את כל התוכן המופרד מהטקסט הגולמי
/// </summary>
public class ParsedDocument
{
    /// <summary>
    /// שורות הטקסט הראשי (לאחר הסרת סימני הערות)
    /// </summary>
    public List<string> MainLines { get; set; } = new();
    
    /// <summary>
    /// מערכת הערות A — מזוהה על ידי $$ בהתחלת שורה
    /// </summary>
    public List<string> FootnotesA { get; set; } = new();
    
    /// <summary>
    /// מערכת הערות B — מזוהה על ידי %% בהתחלת שורה
    /// </summary>
    public List<string> FootnotesB { get; set; } = new();
    
    /// <summary>
    /// מערכת הערות C — מזוהה על ידי && בהתחלת שורה
    /// </summary>
    public List<string> FootnotesC { get; set; } = new();
    
    /// <summary>
    /// שם הספר (לדוג' "בראשית") — מזוהה על ידי === בהתחלת שורה
    /// </summary>
    public string BookName { get; set; } = "";
    
    /// <summary>
    /// שם הפרשה (לדוג' "בראשית") — מזוהה על ידי --- בהתחלת שורה
    /// </summary>
    public string ParashaName { get; set; } = "";
    
    /// <summary>
    /// הטקסט הגולמי המקורי (לצורך דיבוג)
    /// </summary>
    public string RawText { get; set; } = "";
}
