using System.Collections.Concurrent;

using TodoApp.Models;

namespace TodoApp.Data;

/// <summary>
/// Thread-safe in-memory store of to-do items.
/// </summary>
public class TodoStore
{
    private readonly ConcurrentDictionary<int, TodoItem> _items = new();
    private int _nextId = 1;

    public IReadOnlyList<TodoItem> List()
    {
        return _items.Values.OrderBy(i => i.Id).ToList();
    }

    public TodoItem? Get(int id)
    {
        _items.TryGetValue(id, out var item);
        return item;
    }

    public TodoItem Add(string title)
    {
        var item = new TodoItem
        {
            Id = Interlocked.Increment(ref _nextId),
            Title = title,
            Completed = false,
            CreatedAt = DateTime.UtcNow,
        };

        _items[item.Id] = item;
        return item;
    }

    public bool Update(int id, string? title, bool? completed, out TodoItem? updated)
    {
        updated = null;

        if (!_items.TryGetValue(id, out var item))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            item.Title = title;
        }

        if (completed.HasValue)
        {
            item.Completed = completed.Value;
        }

        updated = item;
        return true;
    }

    public bool Delete(int id)
    {
        return _items.TryRemove(id, out _);
    }

    /// <summary>
    /// Removes every completed task. Returns how many were removed.
    /// </summary>
    public int DeleteCompleted()
    {
        var completedIds = _items
            .Where(kv => kv.Value.Completed)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var id in completedIds)
        {
            _items.TryRemove(id, out _);
        }

        return completedIds.Count;
    }

    /// <summary>
    /// Replaces the entire store contents (used when loading from disk).
    /// </summary>
    public void ReplaceAll(IEnumerable<TodoItem> items)
    {
        _items.Clear();

        foreach (var item in items)
        {
            _items[item.Id] = item;
            if (item.Id >= _nextId)
            {
                _nextId = item.Id + 1;
            }
        }
    }
}
