# Handlr Sample WebAPI with Minimal APIs

This sample demonstrates how to use the Handlr CQRS framework with ASP.NET Core minimal APIs.

## 🚀 Features

- **Minimal APIs**: Clean, concise endpoint definitions
- **CQRS Pattern**: Separate commands and queries
- **Pipeline Behaviors**: Logging, validation, and metrics
- **Result Pattern**: Consistent error handling
- **Source Generator**: Automatic handler registration

## 🏗️ Architecture

### Commands (Write Operations)
- `CreateUserCommand` - Creates a new user
- `UpdateUserStatusCommand` - Updates user status  
- `GenerateReportCommand` - Generates reports

### Queries (Read Operations)
- `GetUserByIdQuery` - Retrieves a user by ID
- `GetUsersQuery` - Retrieves users with filtering and pagination

### Pipeline Behaviors
- `LoggingBehavior` - Logs request execution with correlation IDs
- `ValidationBehavior` - Performs basic request validation
- `MetricsBehavior` - Collects performance metrics

## 🔧 Endpoints

### Commands
```http
POST /api/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com", 
  "age": 30
}
```

```http
PUT /api/users/{id}/status
Content-Type: application/json

{
  "status": "Active"
}
```

```http
POST /api/reports
Content-Type: application/json

{
  "startDate": "2024-01-01",
  "endDate": "2024-12-31",
  "reportType": "summary"
}
```

### Queries
```http
GET /api/users/{id}
```

```http
GET /api/users?nameFilter=John&minAge=25&page=1&pageSize=10
```

```http
GET /api/health
```

## 🏃‍♂️ Running the Sample

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run the API:**
   ```bash
   dotnet run
   ```

3. **Test endpoints:**
   ```bash
   # Health check
   curl http://localhost:5244/api/health
   
   # Create user
   curl -X POST http://localhost:5244/api/users \
     -H "Content-Type: application/json" \
     -d '{"name":"John Doe","email":"john@example.com","age":30}'
   
   # Get user
   curl http://localhost:5244/api/users/1234
   
   # Get users with filtering
   curl "http://localhost:5244/api/users?nameFilter=User&page=1&pageSize=5"
   ```

## 📊 Pipeline Behavior Flow

Every request goes through the pipeline in this order:

1. **LoggingBehavior** - Logs request start with correlation ID
2. **ValidationBehavior** - Validates request data
3. **MetricsBehavior** - Tracks performance metrics
4. **Handler** - Executes the actual business logic

Example log output:
```
[a1b2c3d4] Starting CreateUserCommand at 2024-01-15T10:30:00Z
METRIC: CreateUserCommand executed in 45ms [Success]
[a1b2c3d4] Completed CreateUserCommand in 45ms
```

## 🎯 Key Benefits

- **Clean Architecture**: Clear separation between commands and queries
- **Global Behaviors**: Cross-cutting concerns applied automatically
- **Type Safety**: Compile-time guarantees with source generation
- **Performance**: Minimal overhead with direct method calls
- **Testable**: Easy to unit test handlers in isolation

## 🔍 Source Generator

The Handlr source generator automatically:
- Discovers commands and queries
- Generates handler interfaces
- Registers services in DI container
- Creates pipeline execution logic

No manual registration required! Just implement your handlers and they're automatically discovered.