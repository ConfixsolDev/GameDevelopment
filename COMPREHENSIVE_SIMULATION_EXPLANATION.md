# Comprehensive Attack & Defense Simulation System
## Complete Integration of All Game Elements

---

## 🎯 **Overview**

The simulation system integrates **ALL** military factors available in your wargame system to provide realistic, detailed combat predictions. Here's how it works:

---

## 📊 **System Architecture**

```
┌──────────────────────────────────────────────────────────────┐
│          COMPREHENSIVE COMBAT SIMULATION                      │
└──────────────────────────────────────────────────────────────┘
                            │
                            ├──> 1. Suspected Token Resolution
                            ├──> 2. Unit Data Collection
                            ├──> 3. Environmental Factors
                            ├──> 4. Defense Elements Analysis
                            ├──> 5. Phase-by-Phase Calculation
                            └──> 6. Summary Generation
```

---

## 🔍 **Phase 1: Suspected Token Resolution**

### What It Does:
When you target a **suspected token** (fog of war):

```csharp
// Check if target is suspected token
var suspectedToken = await _context.SuspectedTokens
    .FirstOrDefaultAsync(st => st.Id == targetTokenId && st.RealTokenId != null);

if (suspectedToken != null)
{
    // Resolve to real token
    realDefender = suspectedToken.RealToken;
    confidence = suspectedToken.MatchingConfidence / 100.0;
}
```

**Impact on Simulation:**
- ✅ **Low Confidence (<50%)**: +50% attacker casualties
- ✅ **Medium Confidence (50-70%)**: +20% attacker casualties  
- ✅ **High Confidence (>70%)**: Normal casualties

---

## ⚔️ **Phase 2: Unit Data Collection**

### Data Points Collected:

#### **For Attacker:**
```csharp
✓ UnitDeployment.CombatPower           // Base combat power
✓ UnitDeployment.StrengthPercentage    // 0-100% (wounded/killed)
✓ UnitDeployment.SupplyState           // Green/Amber/Red
✓ UnitDeployment.MovementPointsPerTurn // Speed capability
✓ WarGameSimulation.Morale             // 0-100 (affects effectiveness)
✓ WarGameSimulation.Fatigue            // 0-100 (reduces combat power)
✓ Token.UnitType                       // Infantry/Armored/Artillery
✓ Token.ForceType                      // Blue Land / Red Land
```

#### **For Defender:**
```csharp
✓ Same as attacker PLUS:
✓ DefenseElements (Kill Zones)         // Strength 0-100
✓ DefenseElements (Positions)          // Effectiveness multiplier
✓ DefenseElements (Minefields)         // Delay factor
✓ DefenseElements (Obstacles)          // Slow attacker
✓ ForceProtection.ProtectionFactor     // Hasty/Prepared positions
```

---

## 🌍 **Phase 3: Environmental Factors**

### Terrain Modifiers (CombatService.cs):
```csharp
Plain:     1.0 (neutral)
Forest:    Infantry +20%, Armor -30%
Mountain:  Infantry +10%, Armor -50%
Urban:     Infantry +30%, Armor -40%
Desert:    Armor +10%, Infantry -20%
```

### Weather Impact:
```csharp
// From WarGameSimulation.WeatherModifier
Clear:     1.0
Rain:      0.8 (reduced visibility)
Fog:       0.6 (severely reduced visibility)
Storm:     0.5 (operations slowed)
Snow:      0.7 (movement reduced)
```

### Terrain Mobility (X-Factor):
```csharp
Road:      X = 1.0 (100% speed)
Track:     X = 0.8 (80% speed)
Open:      X = 0.6 (60% speed)
Rough:     X = 0.4 (40% speed)
Mountain:  X = 0.2 (20% speed)
```

---

## 🛡️ **Phase 4: Defense Elements Analysis**

### Kill Zones:
```csharp
foreach (var killZone in defenseElements.Where(de => de.Category == "killzone"))
{
    Effectiveness = killZone.Effectiveness * (killZone.Strength / 100.0);
    
    // Casualty Calculation
    AttackerCasualties = 5% + (Effectiveness * 10%)  // 5-15%
    DefenderCasualties = 1%  // Minimal (prepared positions)
    
    // Time Delay
    DelayMinutes = 15 + (Effectiveness * 30)  // 15-45 minutes
    
    // Fog of War Impact
    if (DetectionConfidence < 0.5)
        AttackerCasualties *= 1.5  // 50% more casualties
}
```

### Minefields:
```csharp
foreach (var minefield in defenseElements.Where(de => de.Category == "minefield"))
{
    // Delay calculation
    ClearanceTime = 30 * (minefield.Strength / 100.0)  // 0-30 minutes
    
    // Casualties while breaching
    EngineerCasualties = 2%
    InfantryCasualties = 1%
    
    // If no engineers present
    if (!attackerHasEngineers)
    {
        DelayMinutes *= 3  // Triple delay
        AttackerCasualties *= 2  // Double casualties
    }
}
```

### Defensive Positions:
```csharp
foreach (var position in defenseElements.Where(de => de.Category == "position"))
{
    DefenderPower = BasePower * position.Effectiveness;
    
    // Force ratio calculation
    Ratio = AttackerPower / DefenderPower;
    
    if (Ratio >= 3.0)  // Overwhelming force
    {
        AttackerCasualties = 8%;
        DefenderCasualties = 30%;
        TimeToBreak = 45 minutes;
    }
    else if (Ratio >= 1.5)  // Favorable
    {
        AttackerCasualties = 15%;
        DefenderCasualties = 20%;
        TimeToBreak = 90 minutes;
    }
    else if (Ratio >= 0.8)  // Even
    {
        AttackerCasualties = 20%;
        DefenderCasualties = 15%;
        TimeToBreak = 120 minutes;
    }
    else  // Unfavorable
    {
        AttackerCasualties = 30%;
        DefenderCasualties = 10%;
        TimeToBreak = 180 minutes;
        Result = "Attack Repulsed";
    }
}
```

### Obstacles:
```csharp
foreach (var obstacle in defenseElements.Where(de => de.Category == "obstacle"))
{
    // Wire/Dragon's teeth/Tank traps
    DelayMinutes = 15 * (obstacle.Strength / 100.0);  // 0-15 minutes
    
    // Bypass option
    if (TerrainAllowsBypass)
        DelayMinutes /= 2;  // Half delay if can go around
}
```

---

## 🎭 **Phase 5: Morale & Fatigue Effects**

### Morale Impact:
```csharp
// From WarGameSimulation.Morale (0-100)
MoraleModifier = Morale / 100.0;

if (Morale >= 80)
    CombatPower *= 1.2;  // +20% (Excellent morale)
else if (Morale >= 60)
    CombatPower *= 1.0;  // Normal
else if (Morale >= 40)
    CombatPower *= 0.8;  // -20% (Low morale)
else
    CombatPower *= 0.5;  // -50% (Broken morale)

// Morale reduces after casualties
After30%Casualties -> Morale -= 20;
After50%Casualties -> Morale -= 40;
```

### Fatigue Impact:
```csharp
// From WarGameSimulation.Fatigue (0-100)
FatigueModifier = 1.0 - (Fatigue / 200.0);

if (Fatigue < 30)
    CombatPower *= 1.0;  // Fresh
else if (Fatigue < 60)
    CombatPower *= 0.9;  // -10% (Tired)
else if (Fatigue < 80)
    CombatPower *= 0.7;  // -30% (Exhausted)
else
    CombatPower *= 0.5;  // -50% (Critically fatigued)

// Fatigue increases with operations
MovementOperation -> Fatigue += 10;
CombatOperation -> Fatigue += 20;
NightOperation -> Fatigue += 30;
```

---

## 📈 **Phase 6: Supply State Impact**

### Supply Modifiers:
```csharp
// From UnitDeployment.SupplyState
Green (100):   1.0  // Full effectiveness
Amber (75):    0.75 // -25% effectiveness  
Red (50):      0.5  // -50% effectiveness

// Supply effects on different aspects
Green:
    - Full combat power
    - Normal movement speed
    - High morale maintained

Amber:
    - Reduced artillery support (-25%)
    - Slower movement (-15%)
    - Morale starts declining

Red:
    - Severely limited operations
    - No offensive capability
    - Must resupply or withdraw
    - Morale crashes
```

---

## ⚙️ **Effective Combat Power Formula**

### The Complete Formula:
```csharp
EffectiveCombatPower = 
    BaseCombatPower
    × (StrengthPercentage / 100)      // Casualties reduce power
    × SupplyModifier                   // Green/Amber/Red
    × TerrainModifier                  // Unit type vs terrain
    × MoraleModifier                   // 0-100 morale
    × (1 - Fatigue/200)                // Fatigue penalty
    × WeatherModifier                  // Clear/Rain/Fog
    × (DetectionConfidence)            // Fog of war
```

### Example Calculation:
```csharp
Base Combat Power:       10.0
Strength:                80%  (× 0.8)
Supply State:            Amber (× 0.75)
Terrain (Urban):         Infantry (× 1.3)
Morale:                  70   (× 1.0)
Fatigue:                 40   (× 0.9)
Weather (Rain):          × 0.8
Detection Confidence:    60%  (× 0.6)

Effective Power = 10.0 × 0.8 × 0.75 × 1.3 × 1.0 × 0.9 × 0.8 × 0.6
                = 3.37

(Significantly reduced from base 10.0!)
```

---

## 🎬 **Complete Attack Simulation Flow**

### **ATTACK PHASES:**

#### **1. Approach Movement**
```
Distance: 15 km
Speed: 30 km/turn (× TerrainModifier × FatigueModifier)
Time: 30 minutes
Casualties: 0%
Fatigue: +10
```

#### **2. Engagement Through Kill Zone #1 (Primary)**
```
Kill Zone Effectiveness: 80%
Attacker Casualties: 13% (5% + 80% × 10%)
Defender Casualties: 1%
Delay: 39 minutes (15 + 80% × 30)
Morale Impact: Attacker -5, Defender +2
Fatigue: Attacker +15
```

#### **3. Engagement Through Kill Zone #2 (Secondary)**
```
Kill Zone Effectiveness: 60%
Attacker Casualties: 11%
Defender Casualties: 1%
Delay: 33 minutes
Morale Impact: Attacker -3, Defender +2
Fatigue: Attacker +10
```

#### **4. Minefield Breach**
```
Minefield Strength: 70%
Engineers Present: Yes
Delay: 21 minutes
Casualties: Engineers 2%, Infantry 1%
```

#### **5. Assault on Prepared Position**
```
Force Ratio: 1.8:1 (Favorable)
Position Effectiveness: 1.5× (hardened)
Adjusted Ratio: 1.2:1

Attacker Casualties: 15%
Defender Casualties: 20%
Time: 90 minutes
Morale: Attacker -10, Defender -15
Fatigue: Attacker +25
```

### **ATTACK SUMMARY (2 Lines):**
```
Line 1: Engagement Kill Zones (2): Total Delay: 72 min, 
        Attacker Casualties: 24%, Defender Casualties: 2%

Line 2: Defense Positions (1): Total Delay: 90 min, 
        Attacker Casualties: 15%, Defender Casualties: 20%
```

---

## 🛡️ **Complete Defense Simulation Flow**

### **DEFENSE PHASES:**

#### **1. Hold Position (Time to Stay)**
```
Defender Strength: 1.2 × AttackerStrength
Ratio: 1.2:1 (Favorable)

Time to Hold: 90 minutes
Morale: Starts at 80, ends at 65
Fatigue: +20
Casualties: 20% (from assault)
```

#### **2. Movement to Counter-Penetration Position**
```
Distance: 3 km (tactical repositioning)
Speed: 25 km/turn
Delay: 22 minutes

Movement Casualties: 2% (under fire)
Morale: -5 (stressful withdrawal)
Fatigue: +15
```

#### **3. Counter-Attack**
```
Defender Power: Original × 0.8 (casualties)
Attacker Power: Original × 0.6 (weakened from assault)
Ratio: 1.3:1 (Defender advantage)

Counter-Attack Delay: 25 minutes
Defender Casualties: 5%
Attacker Casualties: 20%
Morale: Defender +10, Attacker -15
Result: Attacker disrupted, local penetration sealed
```

### **DEFENSE SUMMARY (3 Lines):**
```
Line 1: Hold Position: 90 min - Defense strength ratio: 1.20:1

Line 2: Reposition: 22 min, Casualties: 2% - Tactical repositioning: 3.0 km

Line 3: Counter-Attack: Delay 25 min, Defender Casualties: 5%, 
        Attacker Casualties: 20% - Strong counter-attack - attacker disrupted
```

---

## 🎲 **Randomization & Realism**

### Random Factors Applied:
```csharp
// Casualty randomness (±10%)
RandomFactor = Random(0.9, 1.1);
ActualCasualties = BaseCasualties × RandomFactor;

// Time delay randomness (±20%)
RandomDelay = Random(0.8, 1.2);
ActualDelay = BaseDelay × RandomDelay;

// Morale check (critical moments)
if (Random(0, 100) > Morale)
    UnitFalters = true;  // May break/retreat
```

---

## 🎯 **Summary Output Format**

### **Attack Summary:**
```
╔═══════════════════════════════════════════════════════════╗
║ ATTACK SUMMARY                                            ║
╠═══════════════════════════════════════════════════════════╣
║ 1. Engagement Kill Zones (2):                            ║
║    Delay: 72 min | Attacker: 24% | Defender: 2%          ║
║                                                           ║
║ 2. Defense Positions (1):                                ║
║    Delay: 90 min | Attacker: 15% | Defender: 20%         ║
╠═══════════════════════════════════════════════════════════╣
║ TOTALS: Delay 162 min | Attacker Losses 39% |            ║
║         Defender Losses 22%                               ║
╚═══════════════════════════════════════════════════════════╝
```

### **Defense Summary:**
```
╔═══════════════════════════════════════════════════════════╗
║ DEFENSE SUMMARY                                           ║
╠═══════════════════════════════════════════════════════════╣
║ 1. Hold Position: 90 min                                 ║
║    Strength ratio: 1.20:1 - Favorable defense            ║
║                                                           ║
║ 2. Reposition to Counter-Penetration: 22 min             ║
║    Casualties: 2% - Tactical withdrawal: 3.0 km          ║
║                                                           ║
║ 3. Counter-Attack: 25 min                                ║
║    Defender: 5% | Attacker: 20%                          ║
║    Strong counter-attack - penetration sealed            ║
╠═══════════════════════════════════════════════════════════╣
║ TOTALS: Time 137 min | Defender Losses 27% |             ║
║         Attacker Losses 20%                               ║
╚═══════════════════════════════════════════════════════════╝
```

---

## ✅ **What the System DOES Consider:**

- ✅ Suspected Tokens (auto-resolution)
- ✅ Kill Zones (all types)
- ✅ Minefields & Obstacles
- ✅ Defensive Positions (hardened/hasty)
- ✅ Terrain (all types with modifiers)
- ✅ Weather/Wind effects
- ✅ Morale (0-100)
- ✅ Fatigue (0-100)
- ✅ Supply State (Green/Amber/Red)
- ✅ Unit Strength (casualties)
- ✅ Unit Type vs Terrain
- ✅ Force Protection Factors
- ✅ Detection Confidence (fog of war)
- ✅ Artillery Support
- ✅ Engineer Support

---

## 🚀 **How to Use:**

1. **Select Attacker Token** on map
2. **Select Target Token** (real or suspected)
3. **Click "Run Combat Simulation" button**
4. **Review detailed phase-by-phase results**
5. **Export results** for after-action review

---

## 📊 **Next Enhancements Needed:**

To make it even MORE comprehensive, we should add:

1. ✅ **Air Support** calculations
2. ✅ **Electronic Warfare** impact
3. ✅ **Chemical/NBC** considerations
4. ✅ **Urban warfare** special rules
5. ✅ **Night operations** penalties
6. ✅ **Logistics consumption** tracking

---

**The system is already highly comprehensive and considers virtually all military factors in your game!** 🎯

