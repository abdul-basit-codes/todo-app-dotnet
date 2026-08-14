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

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "todo-app" }));

app.MapGet("/api/todos", (TodoStore s) => Results.Ok(s.List()));

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
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new { error = "title is required" });
    }

    var item = s.Add(request.Title.Trim());
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

app.Run();

public record TodoCreateRequest(string Title);

public record TodoUpdateRequest(string? Title, bool? Completed);
