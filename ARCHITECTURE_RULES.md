# Architecture Rules: Modular Design & Single Source of Truth

## 🎯 **Core Principles**

### 1. **Single Source of Truth (SSOT)**
- **Every data operation must go through ONE unified DAL**
- **No direct database access from controllers or services**
- **All business logic centralized in DAL layer**

### 2. **Modular Design**
- **One DAL per domain/entity group**
- **Clear separation of concerns**
- **Consistent patterns across all DALs**

---

## 📋 **DAL Creation Rules**

### **Rule 1: Always Create a Dedicated DAL**
```csharp
// ✅ CORRECT: Create dedicated DAL for each domain
public class UserManagementDAL
public class TokenIdentificationDAL  
public class MapMarkerDAL
public class GameSessionDAL

// ❌ WRONG: Direct database access
public class UserController 
{
    private readonly ApplicationDbContext _context; // DON'T DO THIS
}
```

### **Rule 2: DAL Must Handle ALL Operations for Its Domain**
```csharp
public class TokenIdentificationDAL
{
    // ✅ CRUD Operations
    public async Task<UnifiedSaveResult> SaveTokenAsync(UnifiedTokenSaveRequest request)
    public async Task<UnifiedDeleteResult> DeleteTokenAsync(long tokenId)
    public async Task<UnifiedTokenIdentificationResult> IdentifyTokenAsync(...)
    
    // ✅ Business Logic
    public async Task<List<GroupedTeamTokenInfo>> GetTeamTokensAsync()
    public async Task<bool> ValidateTokenConsistencyAsync(...)
    
    // ✅ Statistics & Analytics
    public async Task<TokenStatistics> GetTokenStatisticsAsync()
}
```

### **Rule 3: Use Unified Request/Response Patterns**
```csharp
// ✅ Unified Request Pattern
public class UnifiedTokenSaveRequest
{
    public long? TokenId { get; set; }        // For updates
    public string Name { get; set; }          // Required
    public string? Description { get; set; }  // Optional
    public bool IsActive { get; set; }        // Default values
    // ... other properties
}

// ✅ Unified Response Pattern
public class UnifiedSaveResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public long? TokenId { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

## 🏗️ **DAL Structure Template**

### **Standard DAL Template**
```csharp
public class [Domain]DAL
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<[Domain]DAL> _logger;
    private readonly IUserSessionService _userSessionService;

    public [Domain]DAL(
        ApplicationDbContext context,
        ILogger<[Domain]DAL> logger,
        IUserSessionService userSessionService)
    {
        _context = context;
        _logger = logger;
        _userSessionService = userSessionService;
    }

    // 1. PRIMARY OPERATIONS (CRUD)
    public async Task<UnifiedSaveResult> Save[Entity]Async(Unified[Entity]SaveRequest request)
    public async Task<UnifiedDeleteResult> Delete[Entity]Async(long id)
    public async Task<[Entity]?> Get[Entity]ByIdAsync(long id)
    public async Task<List<[Entity]>> Get[Entity]sAsync()

    // 2. BUSINESS LOGIC OPERATIONS
    public async Task<[BusinessResult]> [BusinessOperation]Async([parameters])

    // 3. VALIDATION & UTILITY METHODS
    private (bool IsValid, string ErrorMessage) Validate[Operation]Request([Request] request)
    private string GetCurrentTeamId()
    private string GetCurrentUserId()
    private string GetCurrentUserName()

    // 4. PRIVATE HELPER METHODS
    private async Task<[Type]> [HelperMethod]Async([parameters])
}
```

---

## 🔄 **Repository Pattern Rules**

### **Rule 4: Repository Must Use DAL (Not Direct DB Access)**
```csharp
// ✅ CORRECT: Repository uses DAL
public class TokenRepository : ITokenRepository
{
    private readonly ApplicationDbContext _context;
    private readonly TokenIdentificationDAL _tokenDAL; // Use DAL

    public async Task<Token> CreateTokenAsync(Token token)
    {
        // Convert to unified request
        var request = new UnifiedTokenSaveRequest
        {
            Name = token.Name,
            Description = token.Description,
            // ... map properties
        };

        // Use unified DAL
        var result = await _tokenDAL.SaveTokenAsync(request);
        
        if (!result.Success)
            throw new InvalidOperationException(result.Message);

        return await GetTokenByIdAsync(result.TokenId!.Value);
    }
}

// ❌ WRONG: Direct database access
public class TokenRepository
{
    public async Task<Token> CreateTokenAsync(Token token)
    {
        _context.Tokens.Add(token);  // DON'T DO THIS
        await _context.SaveChangesAsync();
        return token;
    }
}
```

---

## 🎛️ **Controller Rules**

### **Rule 5: Controllers Must Use DAL (Not Repository)**
```csharp
// ✅ CORRECT: Controller uses DAL directly
[ApiController]
public class UnifiedTokenController : ControllerBase
{
    private readonly TokenIdentificationDAL _tokenDAL;

    [HttpPost("save")]
    public async Task<ActionResult<UnifiedSaveResult>> SaveToken([FromBody] UnifiedTokenSaveRequest request)
    {
        var result = await _tokenDAL.SaveTokenAsync(request);
        return Ok(result);
    }
}

// ❌ WRONG: Controller uses Repository
public class TokenController : ControllerBase
{
    private readonly TokenRepository _repository; // DON'T DO THIS
}
```

---

## 📊 **Data Flow Architecture**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Controllers   │───▶│      DAL        │───▶│   Database      │
│                 │    │  (Single Source)│    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       ▲
         │                       │
         ▼                       │
┌─────────────────┐              │
│   Repositories  │──────────────┘
│  (DAL Facades)  │
└─────────────────┘
```

---

## 🛡️ **Security & Context Rules**

### **Rule 6: Always Include Team Context**
```csharp
public async Task<UnifiedSaveResult> SaveTokenAsync(UnifiedTokenSaveRequest request)
{
    // ✅ Always get current context
    var teamId = GetCurrentTeamId();
    var userId = GetCurrentUserId();
    
    // ✅ Add context to request
    request.TeamId = teamId;
    request.CreatedByUserId = userId;
    
    // ✅ Validate team access
    if (!await ValidateTeamAccess(teamId))
        return new UnifiedSaveResult { Success = false, Message = "Access denied" };
}
```

### **Rule 7: Use Transactions for Complex Operations**
```csharp
public async Task<UnifiedDeleteResult> DeleteTokenAsync(long tokenId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Delete related data first
        await DeleteRelatedData(tokenId);
        
        // Delete main entity
        await DeleteMainEntity(tokenId);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return new UnifiedDeleteResult { Success = true };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## 📝 **Naming Conventions**

### **DAL Classes**
- `[Domain]DAL` - e.g., `TokenIdentificationDAL`, `UserManagementDAL`

### **Request Classes**
- `Unified[Entity]SaveRequest` - e.g., `UnifiedTokenSaveRequest`
- `Unified[Entity]DeleteRequest` - e.g., `UnifiedTokenDeleteRequest`

### **Response Classes**
- `Unified[Operation]Result` - e.g., `UnifiedSaveResult`, `UnifiedDeleteResult`

### **Methods**
- `Save[Entity]Async()` - Create/Update operations
- `Delete[Entity]Async()` - Delete operations
- `Get[Entity]ByIdAsync()` - Single entity retrieval
- `Get[Entity]sAsync()` - Multiple entities retrieval

---

## 🔍 **Validation Checklist**

Before creating any new DAL, ensure:

- [ ] **Single Responsibility**: DAL handles only one domain
- [ ] **Complete CRUD**: All operations for the domain are included
- [ ] **Unified Patterns**: Uses consistent request/response patterns
- [ ] **Team Context**: All operations respect team isolation
- [ ] **Transaction Safety**: Complex operations use transactions
- [ ] **Error Handling**: Proper exception handling and logging
- [ ] **Validation**: Input validation methods included
- [ ] **Logging**: Comprehensive logging for all operations

---

## 🚀 **Implementation Steps**

### **Step 1: Create DAL Class**
```csharp
public class [Domain]DAL
{
    // Constructor with dependencies
    // Primary CRUD operations
    // Business logic operations
    // Validation methods
    // Helper methods
}
```

### **Step 2: Create Request/Response Classes**
```csharp
public class Unified[Entity]SaveRequest { }
public class Unified[Entity]DeleteRequest { }
public class UnifiedSaveResult { }
public class UnifiedDeleteResult { }
```

### **Step 3: Update Repository (if exists)**
```csharp
public class [Entity]Repository
{
    private readonly [Domain]DAL _dal;
    
    // Route all operations through DAL
}
```

### **Step 4: Update Controllers**
```csharp
public class [Entity]Controller
{
    private readonly [Domain]DAL _dal;
    
    // Use DAL directly, not Repository
}
```

### **Step 5: Register Dependencies**
```csharp
// In Program.cs or Startup.cs
builder.Services.AddScoped<TokenIdentificationDAL>();
builder.Services.AddScoped<UserManagementDAL>();
// ... other DALs
```

---

## 📚 **Examples from Current Codebase**

### **✅ Good Example: TokenIdentificationDAL**
- Single source for all token operations
- Unified request/response patterns
- Team context handling
- Transaction safety
- Comprehensive logging

### **✅ Good Example: TokenRepository Refactor**
- Routes through TokenIdentificationDAL
- Maintains existing interface
- No direct database access

---

## ⚠️ **Common Anti-Patterns to Avoid**

1. **Direct Database Access in Controllers**
2. **Multiple DALs for Same Domain**
3. **Repository Bypassing DAL**
4. **Missing Team Context Validation**
5. **Inconsistent Request/Response Patterns**
6. **Missing Transaction Management**
7. **Insufficient Error Handling**

---

## 🎯 **Benefits of This Architecture**

- ✅ **Single Source of Truth**: All operations go through one place
- ✅ **Consistency**: Unified patterns across all domains
- ✅ **Security**: Team context automatically handled
- ✅ **Maintainability**: Clear separation of concerns
- ✅ **Testability**: Easy to mock and test
- ✅ **Scalability**: Easy to add new operations
- ✅ **Reliability**: Transaction safety and error handling

---

*This document should be updated whenever new patterns or rules are established.*
