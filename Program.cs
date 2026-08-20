using TodoApp.Data;

var builder = WebApplication.CreateBuilder(args);

var store = new TodoStore();
var persistence = new TodoStorePersistence(Path.Combine(AppContext.BaseDirectory, "data", "todos.json"));

var loaded = persistence.Load();
if (loaded is not null)
{
    store.ReplaceAll(loaded);
}

builder.Services.AddSingleton(store);
builder.Services.AddSingleton(persistence);

var app = builder.Build();

app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    Console.WriteLine($"[{DateTime.UtcNow:O}] {context.Request.Method} {context.Request.Path}{context.Request.QueryString} -> {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms");
});

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "todo-app" }));

app.MapGet("/api/todos", (int? limit, int? offset, TodoStore s) =>
{
    var items = s.List();
    if (offset is > 0)
    {
        items = items.Skip(offset.Value).ToList();
    }
    if (limit is > 0)
    {
        items = items.Take(limit.Value).ToList();
    }
    return Results.Ok(items);
});

app.MapGet("/api/todos/stats", (TodoStore s) =>
{
    var items = s.List();
    var completed = items.Count(i => i.Completed);
    return Results.Ok(new
    {
        total = items.Count,
        completed,
        active = items.Count - completed,
        percent = items.Count == 0 ? 0 : (int)Math.Round(completed * 100.0 / items.Count),
        longest = items.OrderByDescending(i => i.Title.Length).FirstOrDefault()?.Title.Length ?? 0,
        newest = items.OrderByDescending(i => i.CreatedAt).FirstOrDefault()?.Id,
    });
});

app.MapGet("/api/todos/search", (string? q, TodoStore s) =>
{
    var items = s.List();
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.Ok(items);
    }

    return Results.Ok(items
        .Where(i => i.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
        .ToList());
});

app.MapGet("/api/todos/summary", (TodoStore s) =>
{
    var items = s.List();
    return Results.Ok(new
    {
        total = items.Count,
        active = items.Count(i => !i.Completed),
        completed = items.Count(i => i.Completed),
        percent = items.Count == 0 ? 0 : (int)Math.Round(items.Count(i => i.Completed) * 100.0 / items.Count),
    });
});

app.MapGet("/api/todos/{id:int}", (int id, TodoStore s) =>
{
    var item = s.Get(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/todos", (TodoCreateRequest request, TodoStore s, TodoStorePersistence p) =>
{
    var title = request.Title?.Trim();
    if (string.IsNullOrWhiteSpace(title))
    {
        return Results.BadRequest(new { error = "title is required" });
    }

    if (title.Length > 120)
    {
        return Results.BadRequest(new { error = "title must be 120 characters or fewer" });
    }

    var item = s.Add(title);
    p.Save(s.List());
    return Results.Created($"/api/todos/{item.Id}", item);
});

app.MapPut("/api/todos/{id:int}", (int id, TodoUpdateRequest request, TodoStore s, TodoStorePersistence p) =>
{
    if (!s.Update(id, request.Title, request.Completed, out var updated))
    {
        return Results.NotFound();
    }

    p.Save(s.List());
    return Results.Ok(updated);
});

app.MapDelete("/api/todos/{id:int}", (int id, TodoStore s, TodoStorePersistence p) =>
{
    if (!s.Delete(id))
    {
        return Results.NotFound();
    }

    p.Save(s.List());
    return Results.Ok(new { deleted = true });
});

app.MapPost("/api/todos/clear-completed", (TodoStore s, TodoStorePersistence p) =>
{
    var removed = s.DeleteCompleted();
    p.Save(s.List());
    return Results.Ok(new { removed, remaining = s.List().Count });
});

app.UseStaticFiles();

app.Lifetime.ApplicationStarted.Register(() =>
    Console.WriteLine($"[startup] TodoApp listening, {store.List().Count} tasks loaded"));
app.Lifetime.ApplicationStopping.Register(() =>
    Console.WriteLine("[shutdown] flushing todo store and stopping"));

app.Run();

public record TodoCreateRequest(string Title);

public record TodoUpdateRequest(string? Title, bool? Completed);
