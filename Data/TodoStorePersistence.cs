using System.Text.Json;

using TodoApp.Models;

namespace TodoApp.Data;

/// <summary>
/// Persists the to-do list to a JSON file on disk.
/// </summary>
public class TodoStorePersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _filePath;

    public TodoStorePersistence(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Loads items from disk, if the file exists.
    /// </summary>
    public List<TodoItem>? Load()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<TodoItem>>(json, JsonOptions);
        }
        catch
        {
            // A corrupt file should never take the app down.
            return null;
        }
    }

    /// <summary>
    /// Writes the full list to disk.
    /// </summary>
    public void Save(IEnumerable<TodoItem> items)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
