# 🚀 Quick Start Guide - NATO Wargame System

## 📌 Quick Access

| Feature | URL | Purpose |
|---------|-----|---------|
| **Game Arena** | `/GamePlay` | Main wargaming interface |
| **Simulation** | `/Home/Simulation` | Run NATO-compliant simulations |
| **Terrain Analysis** | `/Home/TerrainAnalysis` | Analyze terrain impact |
| **AI Analysis** | `/Home/AiAnalysis` | Get AI recommendations |
| **NATO Compliance** | `/Home/NatoCompliance` | Verify system compliance |

---

## 🎮 Quick Start: Attack Planning

1. **Go to** `/GamePlay`
2. **Place tokens** on map (attacker and defender)
3. **Right-click attacker** → "Plan Attack"
4. **Configure attack**:
   - NATO Attack Type: Frontal, Flanking, Envelopment, etc.
   - Attack Intensity: Light, Standard, Heavy, Overwhelming
   - Coordination: Independent, Supporting, Main, etc.
5. **System automatically**:
   - Analyzes terrain
   - Calculates effectiveness
   - Provides AI recommendations
6. **Execute attack** → See NATO symbols on map
7. **View results** in simulation

---

## 🛡️ Quick Start: Defense Planning

1. **Go to** `/GamePlay`
2. **Click on token** → Token details modal opens
3. **Click "Movement Planning" tab** (or use defense controls in overlay)
4. **Create defense elements**:
   - **Kill Zones**: Click "Primary Kill Zone" → Draw polygon
   - **Minefields**: Click "Minefield" → Draw area (shows as NATO dots)
   - **Obstacles**: Click "Obstacle" → Draw line/polygon
   - **Defensive Positions**: Click position type → Place on map
5. **Elements auto-associate** with token
6. **Team visibility** applied automatically
7. **Defense strength** calculated and included in simulation

---

## 🎯 Quick Start: Two-Player Simulation

1. **Go to** `/Home/Simulation`
2. **Configure simulation**:
   - Name: "Test Battle"
   - Max Turns: 20
   - Turn Duration: 30 seconds
   - Terrain: Hills
3. **Add participants**:
   - Participant 1: Type = Attacker, Infantry, Strength 100
   - Participant 2: Type = Defender, Infantry, Strength 80
4. **Click "Start Simulation"**
5. **System runs turn-based simulation**:
   - Player 1 acts in each phase
   - Player 2 acts in each phase
   - Combat resolved automatically
   - AI provides analysis each turn
   - Fog of war applied
6. **View results**:
   - Player 1 ranking and performance
   - Player 2 ranking and performance
   - Comparative analysis
   - Winner determination

---

## 🗺️ Quick Start: Terrain Analysis

1. **Go to** `/Home/TerrainAnalysis`
2. **Select terrain parameters**:
   - Terrain Type: Urban
   - Elevation: 100m
   - Slope: Moderate
   - Vegetation: Dense
   - Drainage: Good
   - Soil: Firm
3. **Click "Analyze Terrain"**
4. **Review results**:
   - Terrain classification (NATO code, classification)
   - Impact coefficients (attack, defense, movement, etc.)
   - Tactical assessment (advantages, disadvantages)
5. **Analyze unit impact**:
   - Select unit type
   - See movement impact for that unit
6. **Generate NATO report**
7. **Export data**

---

## 🤖 Quick Start: AI Analysis

1. **Go to** `/Home/AiAnalysis`
2. **Configure analysis**:
   - Type: Tactical Analysis
   - Scope: Tactical
   - Timeframe: Immediate
   - Priority: High
3. **Add data**:
   - **Forces**: Click "Add Force" → Enter force details
   - **Threats**: Click "Add Threat" → Enter threat details
   - **Opportunities**: Click "Add Opportunity" → Enter details
4. **Select terrain type**
5. **Click "Start AI Analysis"**
6. **Review results**:
   - Analysis summary
   - AI recommendations (immediate, short-term, long-term)
   - Risk assessment
   - Decision support
7. **Generate report**
8. **Export analysis**

---

## 🌐 Quick Start: HLA Interoperability

1. **Go to** `/Home/Simulation`
2. **Toggle "Enable HLA Federation"** → ON
3. **Configure HLA**:
   - Federation Name: `KSA_WARGAME_FEDERATION`
   - Federate Type: `WARGAME_CLIENT`
4. **Click "Connect to Federation"**
5. **Status indicator** changes: 🔴 → 🟢
6. **System now**:
   - Publishes local tokens to federation
   - Receives external entities
   - Visualizes external tokens on map
   - Synchronizes combat events
7. **Disconnect** when done

---

## 📊 Quick Start: NATO Compliance Check

1. **Go to** `/Home/NatoCompliance`
2. **Page automatically**:
   - Checks all 9 systems
   - Verifies NATO standards
   - Displays compliance status
3. **Review**:
   - Overall compliance status (✅ or ⚠️)
   - Systems connected (X / 9)
   - Integration health
   - Each NATO standard status
4. **Click "Generate Compliance Report"** for detailed report
5. **Click "Generate Health Report"** for system health
6. **Click "Export All Reports"** to save

---

## 💡 Pro Tips

### **For Attack Planning**
- Always check terrain analysis before planning attack
- Follow AI recommendations for best results
- Use NATO attack type matching your tactical situation
- Consider attack intensity based on force ratio

### **For Defense Planning**
- Place kill zones in terrain with good fields of fire
- Use minefields on likely enemy approach routes
- Position defensive positions with good cover
- Plan withdrawal routes before engagement

### **For Two-Player Games**
- Use fog of war to your advantage
- Follow AI recommendations carefully
- Monitor enemy strength through detected units
- Balance aggression with force preservation

### **For Analysis**
- Run terrain analysis before major operations
- Use AI analysis for decision support
- Review player rankings to identify improvement areas
- Export reports for after-action review

### **For HLA**
- Only enable when connecting to external systems
- Verify federation name matches external systems
- Monitor connection status indicator
- Disconnect when not needed to reduce overhead

---

## 🔍 Troubleshooting

### **"System not available" error**
- **Check**: All scripts loaded in `/GamePlay`
- **Fix**: Hard refresh browser (Ctrl+F5)
- **Verify**: Check console for script load errors

### **"Not connected to HLA federation" error**
- **Check**: HLA toggle is ON
- **Fix**: Click "Connect to Federation"
- **Verify**: Status shows 🟢 CONNECTED

### **No NATO symbols showing**
- **Check**: Tokens have unitType and organizationLevel
- **Fix**: Edit token and set these fields
- **Verify**: Token should show NATO layout

### **AI analysis not showing**
- **Check**: NATO AI Analysis Engine loaded
- **Fix**: Check console: `window.natoAiAnalysisEngine`
- **Verify**: Should return object, not undefined

### **Terrain effects not applied**
- **Check**: Terrain Integration Service loaded
- **Fix**: Check console: `window.terrainIntegrationService`
- **Verify**: Should show "connected" status

---

## 📞 System Health Check

### **Run in Browser Console**:
```javascript
// Check all systems
window.natoSystemIntegrator.getIntegrationStatus()

// Expected output:
// {
//   systemsConnected: 9,
//   totalSystems: 9,
//   natoCompliant: true,
//   systems: { all true }
// }
```

### **If systems < 9**:
1. Check browser console for script errors
2. Hard refresh page (Ctrl+F5)
3. Verify all scripts in `Views/GamePlay/Index.cshtml`
4. Check network tab for failed script loads

---

## 🎯 Common Workflows

### **Workflow 1: Plan and Execute Attack**
1. Place tokens
2. Check terrain at battle location (`/Home/TerrainAnalysis`)
3. Get AI recommendations (`/Home/AiAnalysis`)
4. Plan attack with NATO intent
5. Execute in simulation
6. Review results

### **Workflow 2: Setup Defensive Position**
1. Place defender token
2. Analyze defensive terrain
3. Create kill zones around position
4. Place minefields on approaches
5. Add defensive positions
6. Plan withdrawal routes
7. Test in simulation

### **Workflow 3: Run Two-Player Game**
1. Setup simulation (participants, terrain, turns)
2. Start simulation
3. Player 1: Plan → Move → Attack
4. Player 2: Plan → Move → Defend
5. System resolves combat
6. AI provides analysis
7. Turn advances
8. Repeat until victory

### **Workflow 4: Analyze Past Simulation**
1. Go to `/Home/Simulation`
2. View simulation history
3. Click simulation → View results
4. Generate NATO analysis report
5. Review player rankings
6. Export for documentation

---

## 📚 Reference

### **NATO Attack Types**
- **Frontal**: Direct assault (⇨)
- **Flanking**: Attack from side (↗)
- **Envelopment**: Surround enemy (↻)
- **Penetration**: Break through lines (⇉)
- **Raid**: Quick strike (⚔)
- **Ambush**: Surprise attack (⚡)

### **NATO Defense Elements**
- **Kill Zone**: Area of concentrated fire
- **Minefield**: Individual mine markers (●)
- **Obstacle**: Physical barriers
- **Defensive Position**: Prepared fighting positions
- **Withdrawal Route**: Planned escape routes
- **Defensive Line**: FEBA, phase lines

### **Terrain Types**
- **Open**: Good for attack, poor for defense
- **Urban**: Excellent for defense, poor mobility
- **Forest**: Good concealment, restricted movement
- **Hills**: Good observation, moderate movement
- **Swamp**: Excellent defense, very poor mobility
- **Desert**: Good movement, poor concealment
- **Mountain**: Excellent defense, very poor mobility
- **Water**: Impassable obstacle

---

## ✅ Final Checklist

Before running your first NATO-compliant simulation:

- [ ] Open `/Home/NatoCompliance` 
- [ ] Verify "Systems Connected" shows 9/9
- [ ] Verify "Overall Compliance Status" shows ✅ NATO COMPLIANT
- [ ] Check each NATO standard shows "Compliant"
- [ ] Review system health shows "OPTIMAL" or "HEALTHY"
- [ ] If any issues, click "Refresh Compliance Check"
- [ ] Generate compliance report for documentation
- [ ] Ready to operate! 🚀

---

## 🎉 You're Ready!

The NATO-integrated wargame system is fully operational. All features are integrated with your existing KSA Wargame platform and ready to use.

**Start with**: `/Home/NatoCompliance` to verify everything is working, then proceed to `/GamePlay` for operations!

**Happy Wargaming!** 🎖️
