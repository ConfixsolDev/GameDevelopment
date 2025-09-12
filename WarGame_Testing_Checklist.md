# War Game System - Testing Checklist

## **📋 QUICK TESTING CHECKLIST**

### **Phase 1: System Setup** ⏱️ 5 minutes
- [ ] **Database Migration**: Run `dotnet ef database update`
- [ ] **Build Success**: Run `dotnet build` - no errors
- [ ] **Application Start**: Run `dotnet run` - application starts
- [ ] **Database Connection**: Verify database connectivity

### **Phase 2: Token & Team Management** ⏱️ 10 minutes
- [ ] **Create Team**: Navigate to `/TeamManagement/Create`
  - [ ] Enter team name: "Blue Force"
  - [ ] Select team color: Blue (#0066CC)
  - [ ] Save successfully
- [ ] **Create Token**: Navigate to `/AdminToken/Create`
  - [ ] Enter token name: "Blue_Tank_01"
  - [ ] Select token type: "Armoured"
  - [ ] Save successfully
- [ ] **Assign User to Team**: Navigate to `/TeamManagement/Members/{teamId}`
  - [ ] Assign current user to Blue Force team
  - [ ] Verify assignment successful

### **Phase 3: Map Integration** ⏱️ 10 minutes
- [ ] **Access Game Arena**: Navigate to `/GamePlay`
  - [ ] Map loads with Leaflet
  - [ ] Control panels visible
  - [ ] Map responds to zoom/pan
- [ ] **Place Token on Map**:
  - [ ] Switch to Blue Force mode
  - [ ] Click on map at coordinates (51.505, -0.09)
  - [ ] Verify token appears with blue color
  - [ ] Verify token has correct icon

### **Phase 4: Military Data Entry** ⏱️ 15 minutes
- [ ] **Access Data Management**: Navigate to `/DataManagement`
  - [ ] Page loads without errors
  - [ ] Brigade table displays
  - [ ] "Add Brigade" button works
- [ ] **Create Brigade**:
  - [ ] Click "Add Brigade"
  - [ ] Enter name: "1st Armored Brigade"
  - [ ] Enter code: "1-ARM-BDE"
  - [ ] Select force type: "Blue"
  - [ ] Save successfully
  - [ ] Verify brigade appears in table
- [ ] **Add Military Units** (via API calls):
  - [ ] Create Infantry Battalion
  - [ ] Create Armoured Regiment
  - [ ] Create Artillery Regiment
  - [ ] Verify all units saved to database

### **Phase 5: War Game Simulation** ⏱️ 20 minutes
- [ ] **Create Scenario**:
  - [ ] Click "Start Sim" in GamePlay
  - [ ] Create scenario: "Tank Battle Exercise 2024"
  - [ ] Verify scenario created
- [ ] **Deploy Units**:
  - [ ] Select units for deployment
  - [ ] Set deployment positions on map
  - [ ] Verify units appear as deployed
- [ ] **Plan Movement**:
  - [ ] Select deployed unit
  - [ ] Click "Plan Move"
  - [ ] Set destination on map
  - [ ] Execute movement order
  - [ ] Verify movement calculation
- [ ] **Initiate Battle**:
  - [ ] Click "Battle" button
  - [ ] Select battle location
  - [ ] Choose participating units
  - [ ] Set battle parameters
  - [ ] Confirm battle setup
- [ ] **Resolve Battle**:
  - [ ] Click "Resolve Battle"
  - [ ] Verify combat calculations
  - [ ] Check battle results
  - [ ] Verify unit strength updates

### **Phase 6: Data Verification** ⏱️ 10 minutes
- [ ] **Data Persistence**:
  - [ ] Refresh browser page
  - [ ] Verify tokens remain on map
  - [ ] Verify brigade data persists
  - [ ] Verify simulation state maintained
- [ ] **Team Isolation**:
  - [ ] Switch to different team (if possible)
  - [ ] Verify only team data visible
  - [ ] Verify cannot edit other team data
- [ ] **Export Data**:
  - [ ] Navigate to `/DataManagement/ExportTeamData`
  - [ ] Verify complete data export
  - [ ] Check all data types included

---

## **🔧 SPECIFIC TEST CASES**

### **Test Case 1: Token Creation to Map Placement**
**Objective**: Verify complete token workflow
**Steps**:
1. Create token via AdminToken interface
2. Place token on map in GamePlay
3. Verify token appears with correct properties
4. Verify token persists after page refresh

**Expected Result**: Token successfully created, placed, and persisted

### **Test Case 2: Brigade Data Entry**
**Objective**: Verify military data management
**Steps**:
1. Access DataManagement interface
2. Create new brigade
3. Add military units to brigade
4. Verify data saves to database
5. Verify data displays in interface

**Expected Result**: All military data successfully created and managed

### **Test Case 3: Combat Simulation**
**Objective**: Verify battle resolution system
**Steps**:
1. Create war game scenario
2. Deploy opposing units
3. Initiate battle between units
4. Resolve battle with combat calculations
5. Verify casualties and victor determination

**Expected Result**: Realistic battle resolution with proper calculations

### **Test Case 4: Team Data Isolation**
**Objective**: Verify security and data isolation
**Steps**:
1. Create data with Team A
2. Switch to Team B user
3. Verify Team A data not visible
4. Verify cannot modify Team A data
5. Verify Team B can only see own data

**Expected Result**: Complete data isolation between teams

---

## **🐛 COMMON ISSUES & SOLUTIONS**

### **Issue 1: Database Migration Errors**
**Symptoms**: Migration fails, foreign key errors
**Solution**: 
```bash
dotnet ef migrations remove
dotnet ef migrations add CompleteWarGameSystem
dotnet ef database update
```

### **Issue 2: Map Not Loading**
**Symptoms**: Blank map area, JavaScript errors
**Solution**: 
- Check Leaflet CSS/JS includes
- Verify map container div exists
- Check browser console for errors

### **Issue 3: Token Not Placing on Map**
**Symptoms**: Click on map does nothing
**Solution**:
- Verify map click event handler
- Check token placement mode is active
- Verify coordinates are valid

### **Issue 4: Data Not Saving**
**Symptoms**: Form submission fails, data not persisted
**Solution**:
- Check AJAX endpoint URLs
- Verify controller methods exist
- Check database connection
- Verify model validation

### **Issue 5: Combat Calculations Not Working**
**Symptoms**: Battle resolution fails, no casualties calculated
**Solution**:
- Check SimulationController methods
- Verify unit data is complete
- Check combat calculation logic
- Verify database relationships

---

## **📊 PERFORMANCE TESTING**

### **Load Testing**
- [ ] Test with 100+ tokens on map
- [ ] Test with 50+ military units
- [ ] Test with 10+ concurrent users
- [ ] Test with large battle scenarios

### **Memory Testing**
- [ ] Monitor memory usage during long sessions
- [ ] Test map performance with many markers
- [ ] Verify no memory leaks in JavaScript

### **Database Performance**
- [ ] Test query performance with large datasets
- [ ] Verify indexes are properly created
- [ ] Test concurrent database access

---

## **✅ SUCCESS CRITERIA**

### **Must Have (Critical)**
- [ ] All database migrations apply successfully
- [ ] All pages load without errors
- [ ] Token creation and placement works
- [ ] Military data entry functions properly
- [ ] Basic combat simulation works
- [ ] Team data isolation enforced

### **Should Have (Important)**
- [ ] Map performance is smooth
- [ ] All forms validate input properly
- [ ] Error messages are helpful
- [ ] Data persists across sessions
- [ ] UI is responsive and intuitive

### **Nice to Have (Enhancement)**
- [ ] Real-time updates
- [ ] Advanced combat visualization
- [ ] Mobile responsiveness
- [ ] Advanced analytics

---

## **📝 TESTING NOTES**

### **Test Environment Setup**
- Use Chrome/Firefox for testing
- Enable browser developer tools
- Clear browser cache between tests
- Use incognito mode for team switching

### **Test Data Guidelines**
- Use realistic military data
- Test with different unit compositions
- Test edge cases (empty data, invalid inputs)
- Test with maximum data limits

### **Documentation Requirements**
- Document all bugs found
- Record performance metrics
- Note any UI/UX issues
- Document any missing features

---

## **🚀 READY FOR PRODUCTION CHECKLIST**

- [ ] All critical functionality works
- [ ] No critical bugs found
- [ ] Performance meets requirements
- [ ] Security requirements met
- [ ] Data integrity verified
- [ ] User experience is acceptable
- [ ] Documentation is complete
- [ ] Deployment process tested

**Final Status**: [ ] READY / [ ] NEEDS WORK

**Notes**: 
_________________________________
_________________________________
_________________________________
