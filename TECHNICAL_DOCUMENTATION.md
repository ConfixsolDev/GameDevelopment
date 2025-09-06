# TechWebSol - Token Management System
## Complete Technical Documentation

### Table of Contents
1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [Database Schema](#database-schema)
4. [API Endpoints](#api-endpoints)
5. [Controllers](#controllers)
6. [Models](#models)
7. [Services](#services)
8. [Frontend Integration](#frontend-integration)
9. [Security](#security)
10. [Development Rules](#development-rules)
11. [Code Standards](#code-standards)
12. [Testing Guidelines](#testing-guidelines)
13. [Deployment](#deployment)

---

## System Overview

TechWebSol is a comprehensive token management system designed for team-based organizations. The system allows administrators to create and manage token groups, assign them to teams, and enable users to interact with tokens through various interfaces including maps, testing, and production environments.

### Key Features
- **Team-based Token Isolation**: Each team has access only to their assigned tokens
- **Hierarchical Token Organization**: Tokens are organized into groups (Company, Brigade, Department, etc.)
- **Game-based Token Lifecycle**: Tokens can be bound to entities during game sessions and freed afterward
- **Unified API Design**: Single API endpoints for consistent behavior across all systems
- **Manual and Physical Tokens**: Support for both touch-based and manually created tokens
- **Administrative Management**: Complete UI for managing teams, groups, and assignments

---

## Architecture

### High-Level Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend UI   │    │   API Layer     │    │   Data Layer    │
│                 │    │                 │    │                 │
│ • Admin Pages   │◄──►│ • Controllers   │◄──►│ • Entity Models │
│ • Token System  │    │ • Services      │    │ • DbContext     │
│ • Game Sessions │    │ • DAL           │    │ • Migrations    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Technology Stack
- **Backend**: ASP.NET Core 8.0, Entity Framework Core
- **Frontend**: Razor Pages, Bootstrap 5, jQuery
- **Database**: SQL Server (configurable)
- **Authentication**: ASP.NET Core Identity
- **API**: RESTful APIs with JSON responses

---

## Database Schema

### Core Tables

#### Users and Teams
```sql
-- Teams table
Teams
├── Id (PK, int)
├── Name (nvarchar(100))
├── TeamCode (nvarchar(50))
├── SubTeamCode (nvarchar(50))
├── Description (nvarchar(500))
├── Category (nvarchar(50))
├── IsActive (bit)
├── CreatedAt (datetime2)
├── CreatedByUserId (nvarchar(50))
└── CreatedByUserName (nvarchar(50))

-- Users table (extends Identity)
AspNetUsers
├── Id (PK, nvarchar(450))
├── UserName (nvarchar(256))
├── Email (nvarchar(256))
├── FirstName (nvarchar(255))
├── LastName (nvarchar(255))
├── TeamId (FK, int) -- NEW
├── TeamCode (nvarchar(50))
├── SubTeamCode (nvarchar(50))
└── ... (other Identity fields)
```

#### Token Management
```sql
-- Token Groups
TokenGroups
├── Id (PK, int)
├── Name (nvarchar(100))
├── GroupCode (nvarchar(50))
├── Category (nvarchar(50))
├── Description (nvarchar(500))
├── IsActive (bit)
└── CreatedAt (datetime2)

-- Tokens
Tokens
├── Id (PK, bigint)
├── Name (nvarchar(100))
├── Description (nvarchar(500))
├── TeamId (nvarchar(50)) -- TeamCode_SubTeamCode
├── TokenGroupId (FK, int)
├── Signature (FK, bigint) -- NULL for manual tokens
├── IsActive (bit)
└── CreatedAt (datetime2)

-- Token Signatures (Physical characteristics)
TokenSignatures
├── Id (PK, bigint)
├── TouchCount (int)
├── Distances (json)
├── Angles (json)
├── Center (json)
└── CreatedAt (datetime2)
```

#### Game Management
```sql
-- Game Sessions
GameSessions
├── Id (PK, int)
├── Name (nvarchar(100))
├── SessionCode (nvarchar(50))
├── Status (nvarchar(20))
├── StartTime (datetime2)
├── EndTime (datetime2)
└── CreatedByUserId (nvarchar(50))

-- Token Bindings
TokenBindings
├── Id (PK, int)
├── GameSessionId (FK, int)
├── TokenGroupId (FK, int)
├── TeamId (nvarchar(50))
├── EntityName (nvarchar(100))
├── IsActive (bit)
└── BoundAt (datetime2)
```

---

## API Endpoints

### Unified Token API (`/api/UnifiedToken`)
| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| POST | `/identify` | Identify token from touch points | `UnifiedTokenIdentificationRequest` | `UnifiedTokenIdentificationResult` |
| POST | `/save` | Save token (create/update) | `UnifiedTokenSaveRequest` | `UnifiedSaveResult` |
| GET | `/team-tokens` | Get team's grouped tokens | None | `List<GroupedTeamTokenInfo>` |

### Admin Token API (`/api/admin/AdminToken`)
| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| POST | `/create-group` | Create token group | `CreateTokenGroupRequest` | `AdminTokenGroupResult` |
| GET | `/groups` | Get all token groups | None | `List<TokenGroupInfo>` |
| POST | `/assign-group-to-team` | Assign group to team | `AssignGroupToTeamRequest` | `AdminAssignmentResult` |
| POST | `/create-manual-token` | Create manual token | `CreateManualTokenRequest` | `ManualTokenResult` |
| GET | `/teams` | Get all teams | None | `List<TeamInfo>` |

### Team Management API (`/api/admin/TeamManagementApi`)
| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| POST | `/create-team` | Create new team | `CreateTeamRequest` | `TeamResult` |
| GET | `/teams` | Get all teams | None | `List<TeamInfo>` |
| GET | `/team-members/{teamId}` | Get team members | None | `List<UserInfo>` |
| POST | `/assign-user-to-team` | Assign user to team | `AssignUserToTeamRequest` | `AssignmentResult` |
| GET | `/users` | Get all users | None | `List<UserInfo>` |

### Game Management API (`/api/game/GameManagement`)
| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| POST | `/start-session` | Start game session | `StartGameSessionRequest` | `GameSessionResult` |
| POST | `/end-session/{id}` | End game session | None | `GameSessionResult` |
| POST | `/bind-tokens` | Bind tokens to entities | `BindTokensRequest` | `BindingResult` |
| GET | `/free-tokens` | Get free tokens | None | `List<FreeTokenInfo>` |
| GET | `/active-sessions` | Get active sessions | None | `List<GameSessionInfo>` |

---

## Controllers

### MVC Controllers (UI)
- **`AdminTokenController`**: Token group management UI
- **`GameManagementController`**: Game session management UI
- **`TeamManagementController`**: Team management UI

### API Controllers
- **`UnifiedTokenController`**: Core token operations
- **`AdminTokenController`** (API): Administrative token operations
- **`TeamManagementApiController`**: Team management operations
- **`GameManagementController`** (API): Game session operations

### Controller Rules
1. **API Controllers** inherit from `ControllerBase`
2. **MVC Controllers** inherit from `Controller`
3. All controllers use `[AuthorizeDynamic]` for authentication
4. API controllers use `[ApiController]` and `[Route]` attributes
5. All controllers inject `ApplicationDbContext` and `IUserSessionService`

---

## Models

### Core Models

#### Token Model
```csharp
public class Token
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TeamId { get; set; } // TeamCode_SubTeamCode
    public int? TokenGroupId { get; set; }
    public TokenSignature? Signature { get; set; } // NULL = manual token
    public bool IsActive { get; set; }
    // ... other properties
}
```

#### Team Model
```csharp
public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TeamCode { get; set; }
    public string? SubTeamCode { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    // ... navigation properties
}
```

### Model Rules
1. **All models** use Data Annotations for validation
2. **Navigation properties** are virtual for EF lazy loading
3. **String properties** have appropriate MaxLength attributes
4. **Required fields** use `[Required]` attribute
5. **Foreign keys** follow naming convention: `{ModelName}Id`

---

## Services

### Data Access Layer (DAL)
- **`TokenIdentificationDAL`**: Centralized token operations
- **`IUserSessionService`**: User session management

### Business Services
- **`IPatternMatchingService`**: Token pattern matching
- **`PatternMatchingService`**: Implementation of pattern matching
- **`PatternAnalysisEngine`**: Advanced pattern analysis

### Service Rules
1. **All services** are registered in `Program.cs`
2. **DAL classes** handle all database operations
3. **Services** are injected via constructor
4. **Async methods** use `async/await` pattern
5. **Error handling** includes proper logging

---

## Frontend Integration

### Razor Pages
- **`/AdminToken/ManageTokenGroups`**: Token group management
- **`/AdminToken/CreateManualToken`**: Manual token creation
- **`/GameManagement/Index`**: Game session management
- **`/TeamManagement/Index`**: Team management

### JavaScript Integration
- **`unified-token-service.js`**: Frontend API client
- **jQuery**: DOM manipulation and AJAX calls
- **Bootstrap 5**: UI framework

### Frontend Rules
1. **All AJAX calls** use jQuery
2. **Form validation** on both client and server
3. **Responsive design** using Bootstrap
4. **Error handling** with user-friendly messages
5. **Loading states** for better UX

---

## Security

### Authentication & Authorization
- **ASP.NET Core Identity** for user management
- **`[AuthorizeDynamic]`** attribute for role-based access
- **Team-based isolation** for data access
- **Secure API endpoints** with authentication

### Data Security
- **SQL injection prevention** via Entity Framework
- **XSS protection** via Razor encoding
- **CSRF protection** via anti-forgery tokens
- **Input validation** on all endpoints

### Security Rules
1. **All controllers** require authentication
2. **API endpoints** validate user permissions
3. **Team isolation** enforced at DAL level
4. **Input sanitization** on all user inputs
5. **Error messages** don't expose sensitive data

---

## Development Rules

### Code Organization
```
TechWebSol/
├── Controllers/
│   ├── TokenManagement/     # API Controllers
│   ├── AdminTokenController.cs
│   └── TeamManagementController.cs
├── Models/
│   ├── Token.cs
│   ├── Team.cs
│   └── ApplicationUser.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── TokenIdentificationDAL.cs
├── Services/
│   └── TokenManagement/
├── Views/
│   ├── AdminToken/
│   ├── GameManagement/
│   └── TeamManagement/
└── wwwroot/
    └── map/js/
```

### Naming Conventions
1. **Controllers**: `{Name}Controller` (MVC) or `{Name}ApiController` (API)
2. **Models**: PascalCase, singular nouns
3. **Methods**: PascalCase, descriptive verbs
4. **Properties**: PascalCase, descriptive nouns
5. **Variables**: camelCase, descriptive names

### File Structure Rules
1. **One class per file**
2. **Namespace matches folder structure**
3. **Using statements** at top of file
4. **XML documentation** for public methods
5. **Consistent indentation** (4 spaces)

---

## Code Standards

### C# Standards
```csharp
// Good example
public async Task<ActionResult<TeamResult>> CreateTeam([FromBody] CreateTeamRequest request)
{
    try
    {
        var currentUser = _userSessionService.GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized("User not authenticated");
        }

        // Validation
        var existingTeam = await _context.Teams
            .FirstOrDefaultAsync(t => t.TeamCode == request.TeamCode);

        if (existingTeam != null)
        {
            return BadRequest($"Team with code '{request.TeamCode}' already exists");
        }

        // Business logic
        var team = new Team
        {
            Name = request.Name,
            TeamCode = request.TeamCode,
            // ... other properties
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return Ok(new TeamResult
        {
            Success = true,
            Message = "Team created successfully",
            Team = new TeamInfo { /* ... */ }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating team");
        return StatusCode(500, "Internal server error");
    }
}
```

### JavaScript Standards
```javascript
// Good example
function createTeam() {
    const data = {
        name: $('#teamName').val(),
        teamCode: $('#teamCode').val(),
        description: $('#teamDescription').val()
    };

    if (!data.name || !data.teamCode) {
        showAlert('Name and Team Code are required', 'warning');
        return;
    }

    $.post('/api/admin/TeamManagementApi/create-team', data)
        .done(function(response) {
            if (response.success) {
                showAlert('Team created successfully', 'success');
                loadTeams();
            } else {
                showAlert(response.message, 'danger');
            }
        })
        .fail(function(xhr) {
            const message = xhr.responseJSON?.message || 'Error creating team';
            showAlert(message, 'danger');
        });
}
```

---

## Testing Guidelines

### Unit Testing
1. **Test all public methods** in controllers and services
2. **Mock dependencies** using Moq framework
3. **Test both success and failure scenarios**
4. **Verify return types and status codes**
5. **Test validation logic**

### Integration Testing
1. **Test API endpoints** with real database
2. **Test authentication and authorization**
3. **Test team isolation** functionality
4. **Test token identification** accuracy
5. **Test game session** lifecycle

### Test Structure
```csharp
[Test]
public async Task CreateTeam_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var request = new CreateTeamRequest
    {
        Name = "Test Team",
        TeamCode = "TEST"
    };

    // Act
    var result = await _controller.CreateTeam(request);

    // Assert
    Assert.IsInstanceOf<OkObjectResult>(result.Result);
    // ... more assertions
}
```

---

## Deployment

### Prerequisites
- .NET 8.0 Runtime
- SQL Server (or compatible database)
- IIS (or compatible web server)

### Configuration
1. **Update connection strings** in `appsettings.json`
2. **Configure authentication** settings
3. **Set up logging** configuration
4. **Configure CORS** if needed
5. **Set up SSL certificates**

### Database Migration
```bash
# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Deployment Steps
1. **Build the application**: `dotnet build --configuration Release`
2. **Publish the application**: `dotnet publish --configuration Release`
3. **Deploy to web server**
4. **Run database migrations**
5. **Configure application settings**

---

## Common Patterns

### API Response Pattern
```csharp
public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
```

### Error Handling Pattern
```csharp
try
{
    // Business logic
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message");
    return StatusCode(500, "Internal server error");
}
```

### Team Isolation Pattern
```csharp
private string GetCurrentTeamId()
{
    var currentUser = _userSessionService.GetCurrentUser();
    var user = _context.Users.FirstOrDefault(u => u.Id == currentUser.ApplicationUserId);
    return $"{user.TeamCode}_{user.SubTeamCode}";
}
```

---

## Troubleshooting

### Common Issues
1. **Build errors**: Check using statements and namespaces
2. **Database errors**: Verify connection string and migrations
3. **Authentication errors**: Check user session service
4. **Team isolation errors**: Verify team assignment
5. **Token identification errors**: Check pattern matching service

### Debugging Tips
1. **Enable detailed logging** in development
2. **Use browser developer tools** for frontend issues
3. **Check database** for data consistency
4. **Verify API responses** using Postman
5. **Check application logs** for error details

---

## Future Enhancements

### Planned Features
1. **Real-time notifications** using SignalR
2. **Advanced analytics** for token usage
3. **Mobile app** integration
4. **Bulk operations** for team management
5. **Audit logging** for all operations

### Scalability Considerations
1. **Database indexing** for performance
2. **Caching** for frequently accessed data
3. **Load balancing** for high availability
4. **Microservices** architecture for large scale
5. **API versioning** for backward compatibility

---

This documentation serves as the complete guide for understanding, maintaining, and extending the TechWebSol Token Management System. Follow these guidelines to ensure consistent, maintainable, and secure code.
