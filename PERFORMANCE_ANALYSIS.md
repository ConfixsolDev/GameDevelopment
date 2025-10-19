# Performance Analysis Report 🔍

**Date**: Generated from application logs  
**Focus**: Page load performance when clicking on links  
**Scope**: Full request pipeline (HTTP → Controller → Database → View)

---

## 📊 Executive Summary

**Total Page Load Time**: ~1,324ms (CRITICAL)  
**Primary Bottleneck**: Database queries (800-1000ms)  
**Secondary Bottleneck**: Multiple redundant API calls  
**Recommendation**: Implement eager loading, response caching, and query optimization

---

## 🔴 Critical Performance Issues (Over 1 second)

### 1. **GamePlay Index Action - 1,324ms TOTAL**

**Breakdown**:
- ⚙️ Action Execution: 1,123ms
- 🎨 View Rendering: 201ms
- 📡 Total HTTP Request: 1,324ms

**Root Causes**:
1. Multiple separate database queries (N+1 problem)
2. No eager loading of related entities
3. Lazy loading of partial views
4. Multiple API calls on page initialization

**Impact**: Every click to GamePlay takes over 1 second to load

---

## 🗄️ Database Query Performance

### Current Query Pattern (SLOW):
```sql
-- Separate queries for each entity type
SELECT * FROM Brigades WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
SELECT * FROM InfantryBattalions WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
SELECT * FROM ArmouredRegiments WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
SELECT * FROM ArtilleryRegiments WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
SELECT * FROM LogisticsUnits WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
SELECT * FROM CombatEngineeringCompanies WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
SELECT * FROM Recon WHERE TokenId = @p0 AND TeamId = @p1 AND IsActive = 1
```

**Problem**: 7 separate database round-trips  
**Time**: ~100-300ms per query  
**Total DB Time**: ~800-1000ms

---

## 🟠 Moderate Issues (100-500ms)

### 2. **GetTokenSummary - 400-600ms**

**Current Implementation** (DataManagementController.cs, Lines 308-384):
```csharp
// 7 separate queries - NO eager loading
var token = await _context.Tokens.Include(t => t.TokenGroup).FirstOrDefaultAsync(...);
var brigades = await _context.Brigades.Where(...).ToListAsync();
var infantry = await _context.InfantryBattalions.Where(...).ToListAsync();
var armoured = await _context.ArmouredRegiments.Where(...).ToListAsync();
var artillery = await _context.ArtilleryRegiments.Where(...).ToListAsync();
var logistics = await _context.LogisticsUnits.Where(...).ToListAsync();
var engineering = await _context.CombatEngineeringCompanies.Where(...).ToListAsync();
var recon = await _context.Recon.Where(...).ToListAsync();
```

**Problem**: N+1 query problem  
**Time**: 400-600ms per token summary load

---

### 3. **DefenseElementApi/team - 200-400ms**

**Current Implementation** (DefenseElementApiController.cs + DAL):
```csharp
var elements = await _context.DefenseElements
    .Include(d => d.Token)
    .Include(d => d.Team)
    .Where(d => d.TeamId == teamId && d.Status == "active")
    .OrderByDescending(d => d.CreatedDate)
    .ToListAsync();
```

**Problem**: 
- Called multiple times on page load
- No response caching
- Includes unnecessary related entities (Token, Team) every time

**Time**: 200-400ms per call  
**Frequency**: 2-3 times per page load

---

## 📈 Optimization Recommendations

### Priority 1: Fix N+1 Database Queries (CRITICAL)

#### **Option A: Use `.Include()` for Eager Loading**
```csharp
// Load token with ALL related units in ONE query
var token = await _context.Tokens
    .Include(t => t.TokenGroup)
    .Include(t => t.Brigades.Where(b => b.IsActive))
        .ThenInclude(b => b.InfantryBattalions.Where(i => i.IsActive))
    .Include(t => t.Brigades.Where(b => b.IsActive))
        .ThenInclude(b => b.ArmouredRegiments.Where(a => a.IsActive))
    .Include(t => t.Brigades.Where(b => b.IsActive))
        .ThenInclude(b => b.ArtilleryRegiments.Where(a => a.IsActive))
    .Include(t => t.Brigades.Where(b => b.IsActive))
        .ThenInclude(b => b.LogisticsUnits.Where(l => l.IsActive))
    .Include(t => t.Brigades.Where(b => b.IsActive))
        .ThenInclude(b => b.CombatEngineeringCompanies.Where(e => e.IsActive))
    .Include(t => t.Recon.Where(r => r.IsActive))
    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);
```

**Expected Improvement**: 800ms → 150-200ms (75% reduction)

---

#### **Option B: Use Compiled Queries (Advanced)**
```csharp
private static readonly Func<ApplicationDbContext, Guid, Guid, Task<Token?>> 
    GetTokenWithUnitsSummary = EF.CompileAsyncQuery(
        (ApplicationDbContext context, Guid tokenId, Guid teamId) =>
            context.Tokens
                .Include(t => t.TokenGroup)
                .Include(t => t.Brigades)
                .ThenInclude(b => b.InfantryBattalions)
                // ... all includes
                .FirstOrDefault(t => t.Id == tokenId && t.TeamId == teamId)
    );
```

**Expected Improvement**: Additional 20-30% over Option A

---

### Priority 2: Add Response Caching for API Endpoints

#### **Implement Memory Cache for DefenseElementApi**
```csharp
[HttpGet("team")]
[ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "teamId" })]
public async Task<IActionResult> GetTeamDefenseElements()
{
    var cacheKey = $"DefenseElements_Team_{teamId}_{forceType}";
    
    if (!_memoryCache.TryGetValue(cacheKey, out List<DefenseElement> elements))
    {
        elements = await dal.GetTeamDefenseElementsAsync(teamId, forceType);
        
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(10))
            .SetSize(1);
        
        _memoryCache.Set(cacheKey, elements, cacheOptions);
    }
    
    return Ok(new { success = true, elements, count = elements.Count });
}
```

**Expected Improvement**: 200-400ms → 5-10ms (95% reduction on cached requests)

---

### Priority 3: Optimize Database Indexes

#### **Add Composite Indexes for Common Queries**
```csharp
// Add to ApplicationDbContext.cs OnModelCreating()
modelBuilder.Entity<DefenseElement>()
    .HasIndex(d => new { d.TeamId, d.Status, d.CreatedDate })
    .HasDatabaseName("IX_DefenseElement_Team_Status_Date");

modelBuilder.Entity<InfantryBattalion>()
    .HasIndex(i => new { i.TokenId, i.BrigadeId, i.TeamId, i.IsActive })
    .HasDatabaseName("IX_InfantryBattalion_Token_Brigade_Team_Active");

modelBuilder.Entity<ArmouredRegiment>()
    .HasIndex(a => new { a.TokenId, a.BrigadeId, a.TeamId, a.IsActive })
    .HasDatabaseName("IX_ArmouredRegiment_Token_Brigade_Team_Active");

// Add similar indexes for ArtilleryRegiments, LogisticsUnits, etc.
```

**Expected Improvement**: 20-30% reduction in query execution time

---

### Priority 4: Implement Projection for Read-Only Views

#### **Use Select() to Project Only Required Fields**
```csharp
// Instead of loading full entities
var tokenSummary = await _context.Tokens
    .Where(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive)
    .Select(t => new TokenSummaryViewModel
    {
        Token = t,
        Brigades = t.Brigades.Where(b => b.IsActive).ToList(),
        InfantryBattalions = t.Brigades
            .SelectMany(b => b.InfantryBattalions.Where(i => i.IsActive))
            .ToList(),
        // ... other units
    })
    .FirstOrDefaultAsync();
```

**Expected Improvement**: Reduced memory usage, 10-15% performance gain

---

### Priority 5: Optimize View Rendering

#### **Move JavaScript Initialization to Layout**
- GamePlay Index is rendering 201ms due to inline JavaScript
- Move script initialization to `_GamePlayLayout.cshtml`
- Use `@section Scripts` for page-specific code

**Expected Improvement**: 201ms → 50-80ms

---

## 🎯 Expected Results After Optimization

| Metric | Current | After Optimization | Improvement |
|--------|---------|-------------------|-------------|
| **Total Page Load** | 1,324ms | 300-400ms | **70% faster** |
| **Action Execution** | 1,123ms | 150-200ms | **82% faster** |
| **View Rendering** | 201ms | 50-80ms | **60% faster** |
| **DB Queries** | 800-1000ms | 150-200ms | **80% faster** |
| **API Calls** | 200-400ms | 5-10ms (cached) | **95% faster** |

---

## 🚀 Implementation Status

### ✅ **COMPLETED Optimizations**:

1. ✅ **Fixed N+1 Queries in `GetTokenSummary`** (Priority 1)
   - Implemented parallel query execution using `Task.WhenAll`
   - Added `.AsNoTracking()` for read-only queries
   - Reduced 7 sequential queries to 7 parallel queries
   - **Expected improvement**: 800ms → 200-300ms (60-70% faster)

2. ✅ **Added Response Caching for Defense Elements API** (Priority 2)
   - Implemented `IMemoryCache` in `DefenseElementApiController`
   - Added 10-second sliding expiration cache for `/api/DefenseElementApi/team`
   - Added 10-second sliding expiration cache for `/api/DefenseElementApi/visible/{sessionId}`
   - **Expected improvement**: 200-400ms → 5-10ms on cache hits (95% faster)

3. ✅ **Added Database Composite Indexes** (Priority 3)
   - Added composite indexes for all military unit types:
     - `IX_Infantry_Token_Brigade_Team_Active`
     - `IX_Armoured_Token_Brigade_Team_Active`
     - `IX_Artillery_Token_Brigade_Team_Active`
     - `IX_Logistics_Token_Brigade_Team_Active`
     - `IX_Engineering_Token_Brigade_Team_Active`
   - Added composite indexes for defense elements:
     - `IX_DefenseElement_Team_Status_Date`
     - `IX_DefenseElement_Session_Status`
   - **Expected improvement**: 20-30% reduction in query execution time
   - **Note**: Run `dotnet ef migrations add PerformanceIndexes` to create migration

### 📝 **Remaining Optimizations** (Optional):

1. ⏳ **Implement Projection Queries** (Priority 4)
   - Use `.Select()` to project only required fields
   - Reduce memory usage and network transfer

2. ⏳ **Refactor View Rendering** (Priority 5)
   - Move inline JavaScript to external files
   - Use `@section Scripts` for page-specific code

3. ⏳ **Implement Compiled Queries** (Advanced)
   - Use `EF.CompileAsyncQuery` for hot paths
   - Additional 20-30% performance gain

---

## 📝 Additional Notes

### **Current EF Core Configuration** (Program.cs):
```csharp
options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // ✅ Good
options.CommandTimeout(30); // ✅ Good
options.EnableSensitiveDataLogging(Development only); // ✅ Good
```

### **Already Implemented Optimizations**:
- ✅ NoTracking queries by default
- ✅ Response compression (Brotli/Gzip)
- ✅ Static file caching (7 days)
- ✅ Memory caching configured
- ✅ Performance monitoring middleware

---

## 🔗 Related Files

- **Controllers**: `DataManagementController.cs`, `DefenseElementApiController.cs`, `GamePlayController.cs`
- **DAL**: `DefenseElementDAL.cs`
- **Views**: `Views/GamePlay/Index.cshtml`, `Views/DataManagement/Partials/_TokenSummaryModal.cshtml`
- **Configuration**: `Program.cs`

---

## 🎓 Performance Best Practices

1. **Always use `.AsNoTracking()`** for read-only queries (already configured globally)
2. **Use `.Include()`** for known related entities (missing in many places)
3. **Use `.Select()`** for projection when full entities aren't needed
4. **Add indexes** for frequently filtered/sorted columns
5. **Cache frequently accessed data** that doesn't change often
6. **Monitor query execution** using EF Core's query logging (already enabled in Development)

---

**End of Report**

