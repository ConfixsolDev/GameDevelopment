# DAL Quick Reference Guide

## 🚀 **Quick Start Checklist**

### **Creating a New DAL**
1. Create `[Domain]DAL` class
2. Add constructor with dependencies
3. Implement CRUD operations
4. Add business logic methods
5. Create request/response classes
6. Register in DI container

### **Updating Existing Repository**
1. Add DAL dependency to constructor
2. Route all operations through DAL
3. Convert entities to unified requests
4. Handle unified responses

---

## 📋 **Standard DAL Template**

```csharp
public class [Domain]DAL
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<[Domain]DAL> _logger;
    private readonly IUserSessionService _userSessionService;

    public [Domain]DAL(ApplicationDbContext context, ILogger<[Domain]DAL> logger, IUserSessionService userSessionService)
    {
        _context = context;
        _logger = logger;
        _userSessionService = userSessionService;
    }

    // PRIMARY OPERATIONS
    public async Task<UnifiedSaveResult> Save[Entity]Async(Unified[Entity]SaveRequest request)
    public async Task<UnifiedDeleteResult> Delete[Entity]Async(long id)
    public async Task<[Entity]?> Get[Entity]ByIdAsync(long id)
    public async Task<List<[Entity]>> Get[Entity]sAsync()

    // BUSINESS LOGIC
    public async Task<[BusinessResult]> [BusinessOperation]Async([parameters])

    // VALIDATION
    private (bool IsValid, string ErrorMessage) Validate[Operation]Request([Request] request)

    // CONTEXT HELPERS
    private string GetCurrentTeamId() => _userSessionService.GetCurrentUser()?.TeamId ?? "default";
    private string GetCurrentUserId() => _userSessionService.GetCurrentUser()?.Id ?? "unknown";
    private string GetCurrentUserName() => _userSessionService.GetCurrentUser()?.UserName ?? "unknown";
}
```

---

## 🔄 **Repository Update Template**

```csharp
public class [Entity]Repository : I[Entity]Repository
{
    private readonly ApplicationDbContext _context;
    private readonly [Domain]DAL _dal; // Add DAL dependency

    public [Entity]Repository(ApplicationDbContext context, [Domain]DAL dal)
    {
        _context = context;
        _dal = dal; // Inject DAL
    }

    public async Task<[Entity]> Create[Entity]Async([Entity] entity)
    {
        // Convert to unified request
        var request = new Unified[Entity]SaveRequest
        {
            // Map properties from entity
        };

        // Use unified DAL
        var result = await _dal.Save[Entity]Async(request);
        
        if (!result.Success)
            throw new InvalidOperationException(result.Message);

        return await Get[Entity]ByIdAsync(result.[Entity]Id!.Value);
    }

    public async Task<bool> Delete[Entity]Async(long id)
    {
        var result = await _dal.Delete[Entity]Async(id);
        return result.Success;
    }
}
```

---

## 📊 **Request/Response Patterns**

### **Save Request Pattern**
```csharp
public class Unified[Entity]SaveRequest
{
    public long? [Entity]Id { get; set; }        // For updates
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? [Entity]GroupId { get; set; }
    
    // Team context (set by DAL)
    internal string TeamId { get; set; } = string.Empty;
    internal string CreatedByUserId { get; set; } = string.Empty;
}
```

### **Save Response Pattern**
```csharp
public class UnifiedSaveResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long? [Entity]Id { get; set; }
    public string? SystemUsed { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### **Delete Response Pattern**
```csharp
public class UnifiedDeleteResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long [Entity]Id { get; set; }
    public int DeletedRelatedCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

---

## 🛡️ **Security Pattern**

```csharp
public async Task<UnifiedSaveResult> Save[Entity]Async(Unified[Entity]SaveRequest request)
{
    try
    {
        // Get current context
        var teamId = GetCurrentTeamId();
        var userId = GetCurrentUserId();
        
        // Add context to request
        request.TeamId = teamId;
        request.CreatedByUserId = userId;

        // Validate request
        var validationResult = ValidateSaveRequest(request);
        if (!validationResult.IsValid)
        {
            return new UnifiedSaveResult
            {
                Success = false,
                Message = validationResult.ErrorMessage
            };
        }

        // Perform operation with team context
        // ... implementation
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in Save[Entity]Async");
        return new UnifiedSaveResult
        {
            Success = false,
            Message = "Error saving [entity]"
        };
    }
}
```

---

## 🔄 **Transaction Pattern**

```csharp
public async Task<UnifiedDeleteResult> Delete[Entity]Async(long id)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Find entity with team context
        var entity = await _context.[Entity]s
            .FirstOrDefaultAsync(e => e.Id == id && e.TeamId == GetCurrentTeamId());

        if (entity == null)
        {
            return new UnifiedDeleteResult
            {
                Success = false,
                Message = "[Entity] not found or access denied",
                [Entity]Id = id
            };
        }

        // Delete related data first
        await DeleteRelatedData(id);

        // Delete main entity
        _context.[Entity]s.Remove(entity);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new UnifiedDeleteResult
        {
            Success = true,
            Message = $"[Entity] '{entity.Name}' deleted successfully",
            [Entity]Id = id
        };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error deleting [entity] {[Entity]Id}", id);
        throw;
    }
}
```

---

## 📝 **DI Registration**

```csharp
// In Program.cs
builder.Services.AddScoped<TokenIdentificationDAL>();
builder.Services.AddScoped<UserManagementDAL>();
builder.Services.AddScoped<MapMarkerDAL>();
// ... other DALs
```

---

## ⚡ **Common Operations**

### **Get with Team Context**
```csharp
public async Task<[Entity]?> Get[Entity]ByIdAsync(long id)
{
    return await _context.[Entity]s
        .Include(e => e.RelatedData)
        .FirstOrDefaultAsync(e => e.Id == id && e.TeamId == GetCurrentTeamId());
}
```

### **List with Team Context**
```csharp
public async Task<List<[Entity]>> Get[Entity]sAsync()
{
    return await _context.[Entity]s
        .Where(e => e.TeamId == GetCurrentTeamId())
        .Include(e => e.RelatedData)
        .OrderByDescending(e => e.CreatedAt)
        .ToListAsync();
}
```

### **Validation Pattern**
```csharp
private (bool IsValid, string ErrorMessage) ValidateSaveRequest(Unified[Entity]SaveRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return (false, "Name is required");

    if (request.Name.Length > 100)
        return (false, "Name cannot exceed 100 characters");

    // Add more validation rules...

    return (true, string.Empty);
}
```

---

## 🎯 **Remember**

- ✅ **Always use DAL for data operations**
- ✅ **Include team context in all operations**
- ✅ **Use transactions for complex operations**
- ✅ **Follow unified request/response patterns**
- ✅ **Add comprehensive logging**
- ✅ **Validate all inputs**
- ✅ **Handle errors gracefully**

---

*Keep this guide handy when working with DALs!*
