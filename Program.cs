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

app.MapGet("/api/todos/{id:int}", (int id, TodoStore s) =>
{
    var item = s.Get(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.UseStaticFiles();

app.Run();
