# Developer Quick Reference Guide
## TechWebSol Token Management System

### 🚀 Getting Started

#### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (LocalDB works for development)

#### Setup
```bash
# Clone repository
git clone <repository-url>
cd TechWebSol

# Restore packages
dotnet restore

# Update database
dotnet ef database update

# Run application
dotnet run
```

### 📁 Project Structure
```
TechWebSol/
├── Controllers/
│   ├── TokenManagement/          # API Controllers
│   │   ├── UnifiedTokenController.cs
│   │   ├── AdminTokenController.cs
│   │   ├── GameManagementController.cs
│   │   └── TeamManagementApiController.cs
│   ├── AdminTokenController.cs   # MVC Controller
│   ├── GameManagementController.cs
│   └── TeamManagementController.cs
├── Models/
│   ├── Token.cs                  # Main token model
│   ├── Team.cs                   # Team model
│   ├── ApplicationUser.cs        # User model
│   ├── TokenGroup.cs             # Token group model
│   └── GameSession.cs            # Game session model
├── Data/
│   ├── ApplicationDbContext.cs   # Database context
│   └── TokenIdentificationDAL.cs # Data access layer
├── Services/
│   └── TokenManagement/          # Business services
├── Views/
│   ├── AdminToken/               # Admin UI pages
│   ├── GameManagement/           # Game UI pages
│   └── TeamManagement/           # Team UI pages
└── wwwroot/
    └── map/js/                   # Frontend JavaScript
```

### 🔧 Common Tasks

#### Adding a New API Endpoint
```csharp
[HttpPost("new-endpoint")]
public async Task<ActionResult<ResultType>> NewEndpoint([FromBody] RequestType request)
{
    try
    {
        var currentUser = _userSessionService.GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized("User not authenticated");
        }

        // Business logic here
        var result = await _someService.DoSomething(request);
        
        return Ok(new ResultType
        {
            Success = true,
            Message = "Operation completed successfully",
            Data = result
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in NewEndpoint");
        return StatusCode(500, "Internal server error");
    }
}
```

#### Adding a New Model
```csharp
[Table("TableName")]
public class NewModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<RelatedModel> RelatedItems { get; set; } = new List<RelatedModel>();
}
```

#### Adding a New View
```html
@{
    ViewData["Title"] = "Page Title";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-icon"></i> Page Title
                    </h3>
                </div>
                <div class="card-body">
                    <!-- Page content here -->
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function() {
            // JavaScript code here
        });
    </script>
}
```

### 🗄️ Database Operations

#### Adding a New DbSet
```csharp
// In ApplicationDbContext.cs
public DbSet<NewModel> NewModels { get; set; }
```

#### Creating a Migration
```bash
dotnet ef migrations add AddNewModel
dotnet ef database update
```

#### Querying Data
```csharp
// Get all active items
var items = await _context.NewModels
    .Where(x => x.IsActive)
    .ToListAsync();

// Get with related data
var itemsWithRelated = await _context.NewModels
    .Include(x => x.RelatedItems)
    .Where(x => x.IsActive)
    .ToListAsync();

// Get by team (for team isolation)
var teamId = GetCurrentTeamId();
var teamItems = await _context.NewModels
    .Where(x => x.TeamId == teamId && x.IsActive)
    .ToListAsync();
```

### 🔐 Security Patterns

#### Team Isolation
```csharp
private string GetCurrentTeamId()
{
    var currentUser = _userSessionService.GetCurrentUser();
    var user = _context.Users.FirstOrDefault(u => u.Id == currentUser.ApplicationUserId);
    return $"{user.TeamCode}_{user.SubTeamCode}";
}

// Always filter by team
var teamId = GetCurrentTeamId();
var data = await _context.SomeTable
    .Where(x => x.TeamId == teamId)
    .ToListAsync();
```

#### Authentication Check
```csharp
var currentUser = _userSessionService.GetCurrentUser();
if (currentUser == null)
{
    return Unauthorized("User not authenticated");
}
```

### 🎨 Frontend Patterns

#### AJAX API Call
```javascript
function callApi() {
    const data = {
        field1: $('#field1').val(),
        field2: $('#field2').val()
    };

    $.post('/api/controller/endpoint', data)
        .done(function(response) {
            if (response.success) {
                showAlert('Success!', 'success');
                // Update UI
            } else {
                showAlert(response.message, 'danger');
            }
        })
        .fail(function(xhr) {
            const message = xhr.responseJSON?.message || 'Error occurred';
            showAlert(message, 'danger');
        });
}
```

#### Form Validation
```javascript
function validateForm() {
    const name = $('#name').val().trim();
    const code = $('#code').val().trim();

    if (!name) {
        showAlert('Name is required', 'warning');
        $('#name').focus();
        return false;
    }

    if (!code) {
        showAlert('Code is required', 'warning');
        $('#code').focus();
        return false;
    }

    return true;
}
```

#### Show Alert
```javascript
function showAlert(message, type) {
    const alertDiv = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    $('.container-fluid').prepend(alertDiv);
    
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);
}
```

### 📋 Common DTOs

#### Request DTO
```csharp
public class CreateItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
}
```

#### Response DTO
```csharp
public class ItemResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ItemInfo? Item { get; set; }
}

public class ItemInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 🐛 Debugging Tips

#### Check Logs
```csharp
_logger.LogInformation("Processing request for user {UserId}", currentUser.ApplicationUserId);
_logger.LogError(ex, "Error processing request");
```

#### Common Issues
1. **Build errors**: Check using statements
2. **Database errors**: Verify connection string
3. **Authentication errors**: Check user session
4. **Team isolation errors**: Verify team assignment
5. **API errors**: Check request/response format

#### Useful Commands
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Check database
dotnet ef database update

# Run with detailed logging
dotnet run --verbosity detailed
```

### 📚 API Reference

#### Token Operations
- `POST /api/UnifiedToken/identify` - Identify token
- `POST /api/UnifiedToken/save` - Save token
- `GET /api/UnifiedToken/team-tokens` - Get team tokens

#### Admin Operations
- `POST /api/admin/AdminToken/create-group` - Create token group
- `GET /api/admin/AdminToken/groups` - Get token groups
- `POST /api/admin/AdminToken/create` - Create manual token

#### Team Operations
- `POST /api/admin/TeamManagementApi/create-team` - Create team
- `GET /api/admin/TeamManagementApi/teams` - Get teams
- `POST /api/admin/TeamManagementApi/assign-user-to-team` - Assign user to team

#### Game Operations
- `POST /api/game/GameManagement/start-session` - Start game session
- `POST /api/game/GameManagement/end-session/{id}` - End game session
- `GET /api/game/GameManagement/active-sessions` - Get active sessions

### 🎯 Best Practices

1. **Always validate input** on both client and server
2. **Use async/await** for all database operations
3. **Implement proper error handling** with logging
4. **Follow team isolation** patterns
5. **Use meaningful variable names**
6. **Add XML documentation** for public methods
7. **Test your changes** before committing
8. **Follow the established patterns** in the codebase

### 📞 Getting Help

1. **Check the logs** for error details
2. **Review the technical documentation**
3. **Look at similar implementations** in the codebase
4. **Test with Postman** for API issues
5. **Check the database** for data consistency

---

This quick reference guide should help you get started quickly with the TechWebSol Token Management System. For detailed information, refer to the complete technical documentation.
