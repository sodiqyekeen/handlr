using SampleWebApi.Queries;

namespace SampleWebApi.Services;

/// <summary>
/// Repository interface for user operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Creates a new user
    /// </summary>
    Task<int> CreateAsync(string name, string email, int age);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<UserDto> GetByIdAsync(int id);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    Task<UserDto> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all users with optional filtering and pagination
    /// </summary>
    Task<List<UserDto>> GetAllAsync(string nameFilter = null, int minAge = 0, int page = 1, int pageSize = 10);

    /// <summary>
    /// Updates a user's status
    /// </summary>
    Task UpdateStatusAsync(int id, string status);
}

/// <summary>
/// In-memory implementation of IUserRepository for demo purposes
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<UserDto> _users = new();
    private int _nextId = 1;

    public InMemoryUserRepository()
    {
        // Seed with some initial data
        _users.AddRange(new[]
        {
            new UserDto { Id = _nextId++, Name = "John Doe", Email = "john@example.com", Age = 30, Status = "Active" },
            new UserDto { Id = _nextId++, Name = "Jane Smith", Email = "jane@example.com", Age = 25, Status = "Active" },
            new UserDto { Id = _nextId++, Name = "Bob Johnson", Email = "bob@example.com", Age = 35, Status = "Inactive" },
            new UserDto { Id = _nextId++, Name = "Alice Brown", Email = "alice@example.com", Age = 28, Status = "Active" },
            new UserDto { Id = _nextId++, Name = "Charlie Wilson", Email = "charlie@example.com", Age = 42, Status = "Active" }
        });
    }

    public Task<int> CreateAsync(string name, string email, int age)
    {
        var user = new UserDto
        {
            Id = _nextId++,
            Name = name,
            Email = email,
            Age = age,
            Status = "Active"
        };

        _users.Add(user);
        return Task.FromResult(user.Id);
    }

    public Task<UserDto> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<UserDto> GetByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<List<UserDto>> GetAllAsync(string nameFilter = null, int minAge = 0, int page = 1, int pageSize = 10)
    {
        var query = _users.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(nameFilter))
            query = query.Where(u => u.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

        if (minAge > 0)
            query = query.Where(u => u.Age >= minAge);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var users = query.Skip(skip).Take(pageSize).ToList();

        return Task.FromResult(users);
    }

    public Task UpdateStatusAsync(int id, string status)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            // Since UserDto is a record, we need to replace it
            var index = _users.IndexOf(user);
            _users[index] = user with { Status = status };
        }

        return Task.CompletedTask;
    }
}