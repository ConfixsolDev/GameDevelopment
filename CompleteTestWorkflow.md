# Complete War Game System Test Workflow

## **SYSTEM STATUS VERIFICATION** ✅

### **Models** ✅ Complete
- ✅ BaseEntity with IsActive, UpdatedAt
- ✅ ApplicationUser with TeamId  
- ✅ Team model
- ✅ Token model
- ✅ Brigade with TokenId foreign key
- ✅ All Military Units (Infantry, Armoured, Artillery)
- ✅ All War Game models (Scenario, Battle, Movement, etc.)

### **Controllers** ✅ Complete  
- ✅ AdminTokenController (Token CRUD)
- ✅ TeamManagementController (Team CRUD)
- ✅ DataManagementController (Military Units + Token Integration)
- ✅ SimulationController (War Game Logic)
- ✅ GamePlayController (Main Interface)

### **Views** ✅ Complete
- ✅ Token creation and management
- ✅ Team creation and assignment  
- ✅ Game play arena with map
- ✅ Brigade data entry modal
- ✅ War game simulation interface

### **JavaScript Integration** ✅ Complete
- ✅ Token selection functionality
- ✅ Map click handlers
- ✅ Brigade data loading
- ✅ Modal integration
- ✅ AJAX API calls

---

## **STEP-BY-STEP TEST EXECUTION**

### **Phase 1: Database Setup** (2 minutes)
```bash
# Apply database migration
dotnet ef migrations add CompleteWarGameSystemV2
dotnet ef database update
```

### **Phase 2: User and Team Setup** (5 minutes)

#### **Step 1: Create Teams**
1. Navigate to `/TeamManagement/Create`
2. Create "Blue Force Team":
   ```json
   {
     "Name": "Blue Force",
     "Description": "NATO Allied Forces", 
     "TeamColor": "#0066CC"
   }
   ```
3. Create "Red Force Team":
   ```json
   {
     "Name": "Red Force",
     "Description": "Opposition Forces",
     "TeamColor": "#CC0000"
   }
   ```

#### **Step 2: Assign Users to Teams**
1. Navigate to `/TeamManagement/Members/{teamId}`
2. Assign current user to Blue Force team
3. Verify team assignment in user profile

### **Phase 3: Token Management** (5 minutes)

#### **Step 3: Create Tokens**
1. Navigate to `/AdminToken/Create`
2. Create tokens for testing:
   ```json
   [
     {
       "TokenName": "Blue_Tank_01",
       "TokenType": "Armoured",
       "Description": "Main Battle Tank Unit"
     },
     {
       "TokenName": "Blue_Infantry_01", 
       "TokenType": "Infantry",
       "Description": "Mechanized Infantry Unit"
     },
     {
       "TokenName": "Red_Tank_01",
       "TokenType": "Armoured", 
       "Description": "Enemy Tank Unit"
     }
   ]
   ```

### **Phase 4: Map Integration and Token Placement** (10 minutes)

#### **Step 4: Access Game Arena**
1. Navigate to `/GamePlay`
2. Verify map loads correctly
3. Verify control panels are visible

#### **Step 5: Place Tokens on Map**
1. Switch to Blue Force mode
2. Click on map to place Blue_Tank_01 at coordinates (51.505, -0.09)
3. Click on map to place Blue_Infantry_01 at coordinates (51.506, -0.08)
4. Switch to Red Force mode  
5. Place Red_Tank_01 at coordinates (51.504, -0.10)
6. Verify all tokens appear on map with correct colors

### **Phase 5: Brigade Data Entry** (15 minutes)

#### **Step 6: Select Token for Data Entry**
1. Click "Select" button in Game Tools
2. Click on Blue_Tank_01 token
3. Verify Token Brigade Data modal opens
4. Verify modal title shows token information

#### **Step 7: Create Brigade Data**
1. In Brigade Details tab, enter:
   ```json
   {
     "Name": "1st Armored Brigade",
     "BrigadeCode": "1-ARM-BDE", 
     "ForceType": "Blue",
     "Description": "Heavy armored formation with tank and mechanized units"
   }
   ```
2. Click Save Brigade
3. Verify success notification

#### **Step 8: Add Infantry Battalion**
1. Switch to Infantry Battalions tab
2. Click "Add Infantry Battalion"
3. Enter data:
   ```json
   {
     "Name": "2nd Mechanized Infantry Battalion",
     "UnitCode": "2-MECH-BN",
     "Strength": 800,
     "Companies": 4,
     "ATGMS": 12,
     "Mortars81mm": 6,
     "MarchingSpeedTrucksRoads": 30,
     "MarchingSpeedAPCs": 20
   }
   ```
4. Save and verify unit appears in table

#### **Step 9: Add Armoured Regiment**
1. Switch to Armoured Regiments tab
2. Add regiment:
   ```json
   {
     "Name": "1st Tank Regiment",
     "UnitCode": "1-TK-RGT", 
     "Strength": 400,
     "Squadrons": 3,
     "Tanks": 44,
     "MarchingSpeedRoads": 15,
     "MarchingSpeedCrossCountry": 10
   }
   ```

#### **Step 10: Add Artillery Regiment**
1. Switch to Artillery Regiments tab
2. Add regiment:
   ```json
   {
     "Name": "1st Artillery Regiment",
     "UnitCode": "1-ART-RGT",
     "Strength": 300, 
     "Batteries": 3,
     "Guns": 18,
     "GunRange": 30,
     "GunCaliber": "155mm SP"
   }
   ```

### **Phase 6: War Game Simulation** (20 minutes)

#### **Step 11: Create War Game Scenario**
1. Click "Start Sim" button
2. In Simulation panel, click "New Scenario"
3. Create scenario:
   ```json
   {
     "Name": "Tank Battle Exercise 2024",
     "Description": "Armored engagement simulation",
     "ScenarioCode": "TBE-2024-01"
   }
   ```

#### **Step 12: Deploy Units**
1. Switch to Units tab in simulation panel
2. Click "Deploy Unit"
3. Select Blue_Tank_01 brigade units for deployment
4. Set deployment position on map
5. Repeat for Red_Tank_01 units
6. Verify units appear as deployed on map

#### **Step 13: Plan Movement**
1. Select deployed Blue unit
2. Click "Plan Move"  
3. Select destination on map
4. Choose movement type: "Tactical"
5. Execute movement order
6. Verify movement calculation shows time and distance

#### **Step 14: Initiate Battle**
1. Click "Battle" button
2. Select battle location between opposing units
3. Choose participating units from both sides
4. Set battle type: "Engagement"
5. Set terrain: "Plain"
6. Set weather: "Clear"
7. Confirm battle setup

#### **Step 15: Resolve Battle**
1. Click "Resolve Battle"
2. Verify combat calculations execute
3. Check battle results show:
   - Casualties for both sides
   - Victor determination
   - Combat effectiveness factors
   - Terrain and weather modifiers
4. Verify unit strengths updated based on casualties

### **Phase 7: Verification and Testing** (10 minutes)

#### **Step 16: Data Persistence Check**
1. Refresh browser page
2. Verify tokens remain on map
3. Select token and verify brigade data persists
4. Check simulation state is maintained

#### **Step 17: Team Isolation Check**
1. Switch user to different team (if possible)
2. Verify only team-owned data is visible
3. Verify cannot edit other team's data

#### **Step 18: Export Data**
1. Navigate to `/DataManagement/ExportTeamData`
2. Verify complete data export includes:
   - Brigades with token associations
   - All military units
   - Battle results
   - Movement history

---

## **SUCCESS CRITERIA CHECKLIST**

### **Core Functionality** ✅
- [ ] Database migration applies successfully
- [ ] Teams can be created and users assigned
- [ ] Tokens can be created and placed on map
- [ ] Token selection opens brigade data entry
- [ ] Brigade and military unit data can be created/edited
- [ ] War game scenarios can be created
- [ ] Units can be deployed and moved
- [ ] Battles can be initiated and resolved
- [ ] Combat calculations produce realistic results

### **Data Integrity** ✅
- [ ] All data is properly associated with teams
- [ ] Token-Brigade relationships work correctly
- [ ] Military units link to correct brigades
- [ ] Battle results update unit strengths
- [ ] Data persists across browser sessions

### **User Experience** ✅
- [ ] Map interface is responsive and intuitive
- [ ] Modals open and close smoothly
- [ ] Forms validate input properly
- [ ] Success/error notifications display
- [ ] All buttons and controls work as expected

### **Security** ✅
- [ ] Team-based data isolation enforced
- [ ] Users can only access their team's data
- [ ] Unauthorized access properly blocked
- [ ] Input validation prevents malicious data

---

## **KNOWN LIMITATIONS**

1. **Real-time Updates**: Map doesn't update in real-time for other users
2. **Advanced Combat**: Combat model is simplified Lanchester equations
3. **Terrain Integration**: Terrain factors are basic, not GIS-integrated
4. **Mobile Support**: Interface optimized for desktop use

---

## **NEXT ENHANCEMENT OPPORTUNITIES**

1. **Real-time Collaboration**: WebSocket integration for live updates
2. **Advanced AI**: AI-controlled opposing forces
3. **3D Visualization**: Three.js integration for 3D battle view
4. **Historical Analysis**: Battle replay and analysis tools
5. **Mobile App**: Dedicated mobile interface for field use

---

## **FINAL VERIFICATION COMMAND**

```bash
# Build and run the complete system
dotnet build
dotnet run

# Navigate to: http://localhost:5000/GamePlay
# Execute complete test workflow above
# Verify all success criteria met
```

**Expected Result**: Complete end-to-end war game system operational with all features working from token creation to battle resolution.
