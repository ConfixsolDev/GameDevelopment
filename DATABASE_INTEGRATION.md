# 💾 Database Integration - Defense Elements

## ✅ Complete Database Integration Implemented

Defense elements (kill zones, minefields, obstacles, etc.) are now **fully integrated** with the database, similar to how tokens are saved and loaded.

---

## 📊 Database Schema

### **DefenseElement Table**

| Column | Type | Description |
|--------|------|-------------|
| `Id` | Guid (PK) | Primary key |
| `ElementId` | string | Client-side unique identifier |
| `Category` | string | killzone, minefield, obstacle, position, route, line |
| `Type` | string | primary, secondary, antipersonnel, etc. |
| `Coordinates` | nvarchar(max) | JSON array of coordinates [[lat, lng], ...] |
| **`TokenId`** | **int?** | **Token responsible for this element** |
| `TeamId` | int? | Team ownership |
| `GameSessionId` | int? | Game session association |
| `Strength` | int | Defense strength (0-100) |
| `Effectiveness` | double | Effectiveness multiplier (0.5-2.0) |
| `Visibility` | string | friendly, control, force, all |
| `Status` | string | active, destroyed, inactive |
| `CreatedByUserId` | Guid | User who created |
| `Notes` | string | Optional notes |
| `Metadata` | nvarchar(max) | Additional JSON metadata |
| `CreatedDate` | DateTime | Creation timestamp |
| `ModifiedDate` | DateTime | Last modified timestamp |

### **Relationships**
- `DefenseElement` → `Token` (Many-to-One) - **Token responsibility assignment**
- `DefenseElement` → `Team` (Many-to-One) - Team ownership
- `DefenseElement` → `GameSession` (Many-to-One) - Session context

---

## 🔧 Implementation Files

### **1. Database Model**
**File**: `Models/DefenseElement.cs`
- Complete model with all properties
- Foreign keys to Token, Team, GameSession
- Validation attributes
- Navigation properties

### **2. Data Access Layer (DAL)**
**File**: `DAL/DefenseElementDAL.cs`

**Methods**:
- `GetDefenseElementsBySessionAsync()` - Get all for session
- `GetVisibleDefenseElementsAsync()` - Team visibility filtering
- `GetDefenseElementsByTokenAsync()` - **Get all elements for a token**
- `CreateDefenseElementAsync()` - Create new element
- `UpdateDefenseElementAsync()` - Update existing
- `AssociateWithTokenAsync()` - **Assign element to token**
- `DissociateFromTokenAsync()` - Remove token assignment
- `CalculateTokenDefenseStrengthAsync()` - **Calculate total defense strength**
- `DeleteDefenseElementAsync()` - Soft delete
- `GetDefenseElementsByCategoryAsync()` - Filter by category
- `CreateDefenseElementsBulkAsync()` - Bulk create

### **3. API Controller**
**File**: `Controllers/DefenseElementApiController.cs`

**Endpoints**:
```
GET    /api/DefenseElementApi/session/{gameSessionId}
GET    /api/DefenseElementApi/visible/{gameSessionId}          ← Team-filtered
GET    /api/DefenseElementApi/token/{tokenId}                  ← Get elements for token
POST   /api/DefenseElementApi/create
PUT    /api/DefenseElementApi/update/{id}
POST   /api/DefenseElementApi/associate/{elementId}/token/{tokenId}  ← Assign to token
POST   /api/DefenseElementApi/dissociate/{elementId}
DELETE /api/DefenseElementApi/delete/{id}
GET    /api/DefenseElementApi/strength/{tokenId}               ← Get total strength
GET    /api/DefenseElementApi/category/{gameSessionId}/{category}
```

### **4. JavaScript Integration**
**File**: `wwwroot/js/DefensePlanningManager.js`

**Methods**:
- `loadDefenseElements()` - **Single function to load everything from DB**
- `saveDefenseElementToDatabase()` - Auto-save when created
- `updateTokenAssociationInDatabase()` - Save token assignment
- `reconstructDefenseElement()` - Rebuild element on map from DB data
- `clearAllLayers()` - Clear before reload
- `getCurrentGameSessionId()` - Get session ID

### **5. Auto-Load Integration**
**File**: `wwwroot/js/gamePlayManager.js`

Added to `loadBackgroundData()`:
```javascript
await Promise.allSettled([
    this.restorePlacedTokens(),
    this.loadAttackOrdersAfterTokensPlaced(),
    this.loadDefenseElements(),    // ← NEW: Load defense elements
    this.preloadCriticalModals()
]);
```

### **6. Database Context**
**File**: `Data/ApplicationDbContext.cs`
- Added `DbSet<DefenseElement> DefenseElements`
- Entity configuration with indexes
- Foreign key relationships

---

## 🔄 Data Flow

### **Create Defense Element**
```
User draws kill zone on map
    ↓
JavaScript creates local visualization
    ↓
saveDefenseElementToDatabase() called automatically
    ↓
POST /api/DefenseElementApi/create
    ↓
DefenseElementDAL.CreateDefenseElementAsync()
    ↓
Saved to database with TeamId, CreatedByUserId
    ↓
Returns database ID (stored as element.dbId)
```

### **Associate with Token (Assign Responsibility)**
```
User associates element with token
    ↓
JavaScript updates local tokenId
    ↓
updateTokenAssociationInDatabase() called automatically
    ↓
POST /api/DefenseElementApi/associate/{elementId}/token/{tokenId}
    ↓
DefenseElementDAL.AssociateWithTokenAsync()
    ↓
TokenId saved in database
    ↓
Total defense strength calculated and returned
```

### **Load on Page Refresh**
```
Page loads
    ↓
gamePlayManager.loadBackgroundData()
    ↓
gamePlayManager.loadDefenseElements()
    ↓
defensePlanningManager.loadDefenseElements(gameSessionId)
    ↓
GET /api/DefenseElementApi/visible/{gameSessionId}
    ↓
DefenseElementDAL.GetVisibleDefenseElementsAsync() - with team filtering
    ↓
Returns elements user can see based on controlType
    ↓
reconstructDefenseElement() for each element
    ↓
Creates visual layers on map
    ↓
Stores locally in defenseElements Map
    ↓
Defense elements appear on map with correct visibility
```

---

## 🎯 Token Responsibility Assignment

### **When Creating Defense Element**
```javascript
// Element created WITHOUT token assignment initially
{
    elementId: "defense_123",
    category: "killzone",
    type: "primary",
    coordinates: [[12.751, 44.864], ...],
    tokenId: null,              // ← No assignment yet
    teamId: 1,
    strength: 100
}
```

### **When Associating with Token**
```javascript
// User clicks "Associate with Token" in UI
await defensePlanningManager.associateWithToken("defense_123", tokenId);

// Updates in database:
UPDATE DefenseElements 
SET TokenId = 5, ModifiedDate = NOW() 
WHERE ElementId = "defense_123"

// Now element shows responsibility:
{
    elementId: "defense_123",
    category: "killzone",
    type: "primary",
    tokenId: 5,                 // ← Now assigned to Token ID 5
    dbId: 42                    // ← Database primary key
}
```

### **Calculating Token Defense Strength**
```javascript
// Get all defense elements for a token
GET /api/DefenseElementApi/token/5

// Returns:
{
    elements: [
        { category: "killzone", strength: 100 },    // +30 (30% contribution)
        { category: "minefield", strength: 100 },   // +40 (40% contribution)
        { category: "obstacle", strength: 80 }      // +16 (20% contribution)
    ],
    totalDefenseStrength: 86
}
```

---

## 🔒 Team Visibility Rules

Implemented in `DefenseElementDAL.GetVisibleDefenseElementsAsync()`:

### **Force Control Type**
```csharp
// Can see EVERYTHING (both teams)
if (controlType == "force") {
    return all defense elements;
}
```

### **Control Type**
```csharp
// Can see own team + elements marked as "control" or "all"
if (controlType == "control") {
    return elements where (
        TeamId == userTeamId OR 
        Visibility == "control" OR 
        Visibility == "all"
    );
}
```

### **Friendly/Other**
```csharp
// Can only see own team's elements
return elements where TeamId == userTeamId;
```

---

## 🎮 Usage Example

### **Creating and Associating Defense Elements**
```javascript
// 1. User creates kill zone
startKillZoneDrawing('primary');  // Draw on map
// ✅ Auto-saved to database

// 2. User associates with token
await defensePlanningManager.associateWithToken("defense_123", tokenId);
// ✅ Token assignment saved to database
// ✅ Total defense strength calculated: 86

// 3. Page refresh
location.reload();
// ✅ Defense elements auto-load from database
// ✅ Kill zone appears on map
// ✅ Token association preserved
// ✅ Team visibility enforced
```

### **Checking Token Defense**
```javascript
// Get all defense elements for a token
const response = await fetch('/api/DefenseElementApi/token/5');
const data = await response.json();

console.log('Defense elements:', data.elements);
console.log('Total defense strength:', data.totalDefenseStrength);

// Each element shows:
// - category, type, coordinates
// - tokenId (responsibility assignment)
// - strength, effectiveness
// - teamId, visibility
```

---

## 📋 Migration Required

Run these commands to create the database table:

```bash
dotnet ef migrations add AddDefenseElementsTable
dotnet ef database update
```

**Migration will create**:
- DefenseElements table
- Foreign keys to Tokens, Teams, GameSessions
- Indexes on ElementId, Category, TokenId, GameSessionId, Status

---

## ✅ Verification Steps

### **Step 1: Create Defense Element**
1. Go to `/GamePlay`
2. Click "Primary Kill Zone" button
3. Draw polygon on map
4. **Check database**: `SELECT * FROM DefenseElements` - should see new row

### **Step 2: Associate with Token**
1. Right-click on token → "Associate Defense Elements"
2. Select kill zone
3. **Check database**: `SELECT TokenId FROM DefenseElements WHERE ElementId = 'defense_123'` - should show token ID

### **Step 3: Refresh Page**
1. Press F5 to refresh
2. **Check console**: Should see "📥 Loaded X defense elements from database"
3. **Check map**: Kill zone should reappear in same location
4. **Check token**: Token should still show association

### **Step 4: Team Visibility**
1. Login as different team user
2. Refresh page
3. **Check**: Should only see defense elements for that team (unless control type = "force")

---

## 🎯 Summary

**✅ YES - Everything is now saved to database with TokenId:**

1. ✅ Defense elements **auto-save** when created
2. ✅ Token assignment **saves to database** via `TokenId` column
3. ✅ Defense elements **auto-load** on page refresh via single `loadDefenseElements()` call
4. ✅ Team visibility **enforced** at database level
5. ✅ Defense strength **calculated** from all elements assigned to token
6. ✅ Full CRUD operations via API
7. ✅ Integrated with existing game session workflow
8. ✅ Same pattern as token placement loading

**Single function call**: `window.defensePlanningManager.loadDefenseElements(gameSessionId)`

**Auto-loads on page refresh** through `gamePlayManager.loadBackgroundData()`! 🚀
