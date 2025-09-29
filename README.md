# Caseworker Tasks API

simple task management system for caseworkers

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQLite (included with .NET)

### Running the Application

```bash
# Clone and navigate to the project
cd dts-developer-challenge

# Restore dependencies
dotnet restore

# Run the API (database will be created automatically)
dotnet run --project src/CaseworkerTasks.Api

# Run tests
dotnet test
```

The API will start on `https://localhost:7124` with:
- Database automatically created and seeded with sample data in development
- Interactive API documentation at `/swagger`
- Health check endpoint at `/health`

### Database Setup

The application uses SQLite with Entity Framework Core:
- **Development**: Database (`tasks.db`) is created automatically on startup
- **Sample Data**: 5 realistic caseworker tasks are seeded in development mode
- **Testing**: In-memory database for isolated test runs

No manual migration commands needed - everything happens automatically!

## API Documentation

Visit https://localhost:7124/swagger for interactive API documentation.

### Available Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check with database connectivity |
| GET | `/` | Welcome message with available endpoints |
| POST | `/tasks` | Create a new task |
| GET | `/tasks` | Get all tasks (sorted by due date) |
| GET | `/tasks/{id}` | Get a specific task |
| PATCH | `/tasks/{id}/status` | Update task status |
| DELETE | `/tasks/{id}` | Delete a task |

### Task Status Values
- `ToDo` - Task not started
- `InProgress` - Task in progress
- `Done` - Task completed