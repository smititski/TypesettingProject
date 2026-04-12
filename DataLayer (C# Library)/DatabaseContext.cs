using SQLite;

namespace DataLayer;

/// <summary>
/// Model representing text content stored in the database.
/// </summary>
public class TextContent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Database context for SQLite operations.
/// </summary>
public class DatabaseContext
{
    private readonly SQLiteConnection _connection;

    public DatabaseContext(string databasePath)
    {
        _connection = new SQLiteConnection(databasePath);
        InitializeTables();
    }

    private void InitializeTables()
    {
        _connection.CreateTable<TextContent>();
    }

    /// <summary>
    /// Save text content to the database.
    /// </summary>
    public int SaveContent(string content)
    {
        var textContent = new TextContent
        {
            Content = content,
            UpdatedAt = DateTime.UtcNow
        };
        return _connection.Insert(textContent);
    }

    /// <summary>
    /// Get all text content from the database.
    /// </summary>
    public List<TextContent> GetAllContent()
    {
        return _connection.Table<TextContent>().ToList();
    }

    /// <summary>
    /// Get a specific text content by ID.
    /// </summary>
    public TextContent? GetContent(int id)
    {
        return _connection.Get<TextContent>(id);
    }

    /// <summary>
    /// Update existing text content.
    /// </summary>
    public void UpdateContent(TextContent content)
    {
        content.UpdatedAt = DateTime.UtcNow;
        _connection.Update(content);
    }

    /// <summary>
    /// Delete text content by ID.
    /// </summary>
    public void DeleteContent(int id)
    {
        _connection.Delete<TextContent>(id);
    }

    /// <summary>
    /// Close the database connection.
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
    }
}