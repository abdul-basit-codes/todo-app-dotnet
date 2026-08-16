# TodoApp

A fully working full-stack to-do application:

- **Backend**: ASP.NET Core minimal API (C#) with a thread-safe in-memory store
  and JSON file persistence.
- **Frontend**: A polished, responsive single-page UI (HTML / CSS / JavaScript)
  with a professional dark theme, filters, and animated task states.
- **No external NuGet packages** — pure ASP.NET Core framework reference.

## Features

- Create, read, update, and delete tasks
- Toggle tasks between active and completed
- Filter by all / active / completed
- Task count summary + animated completion progress bar
- Inline title editing (click a task title to rename)
- Clear completed tasks in one click
- Keyboard-friendly form (Enter to add)
- Persistence: tasks survive server restarts via `data/todos.json`

## API

| Method | Route             | Description              |
|--------|-------------------|--------------------------|
| GET    | /api/todos        | List all tasks           |
| GET    | /api/todos/search?q=term | Search tasks by title |
| GET    | /api/todos/summary | Progress summary (counts + percent) |
| GET    | /api/todos/:id    | Get one task             |
| POST   | /api/todos        | Create a task            |
| POST   | /api/todos/clear-completed | Delete all completed tasks |
| PUT    | /api/todos/:id    | Update title / completed |
| DELETE | /api/todos/:id    | Delete a task            |
| GET    | /api/health       | Health check             |

Task shape:

```json
{
  "id": 7,
  "title": "Write docs",
  "completed": false,
  "createdAt": "2026-08-12T10:00:00Z"
}
```

## Build and run

Requires the .NET 10 SDK (or .NET 8+).

```sh
dotnet build
dotnet run
```

Then open http://localhost:5123 in your browser (check the console for the
exact port).

## Example requests

```sh
curl http://localhost:5123/api/todos
curl -X POST http://localhost:5123/api/todos -H "Content-Type: application/json" -d '{"title":"Ship v2"}'
curl "http://localhost:5123/api/todos/search?q=ship"
curl -X POST http://localhost:5123/api/todos/clear-completed
curl http://localhost:5123/api/todos/summary
```

## Run with Docker

```sh
docker build -t todo-app .
docker run --rm -p 8080:8080 todo-app
```

Then open http://localhost:8080.

## Project layout

```
todo-app-dotnet/
├── Models/
│   └── TodoItem.cs          # Task model
├── Data/
│   └── TodoStore.cs         # Thread-safe in-memory store
│   └── TodoStorePersistence.cs  # JSON file persistence
├── wwwroot/                 # Frontend (served statically)
│   ├── index.html
│   ├── css/site.css
│   └── js/app.js
├── Program.cs               # Minimal API + startup wiring
└── TodoApp.csproj
```

## License

MIT
