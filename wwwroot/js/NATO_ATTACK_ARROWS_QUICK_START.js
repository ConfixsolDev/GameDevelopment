/**
 * NATO Attack Arrows - Quick Start Guide
 * Example code for creating and managing NATO-style attack arrows
 */

// ============================================
// EXAMPLE 1: Create Simple Attack Arrow
// ============================================
function example1_SimpleAttackArrow() {
    console.log('📋 Example 1: Creating simple attack arrow');
    
    // Get two tokens from the map
    const tokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    if (tokens.length < 2) {
        console.error('Need at least 2 tokens on the map');
        return;
    }
    
    const attacker = tokens[0].tokenData;
    const target = tokens[1].tokenData;
    
    // Create attack arrow
    const attackId = window.attackVisualizationManager.addAttackLine(attacker, target);
    
    console.log('✅ Attack arrow created:', attackId);
    console.log('   Attacker:', attacker.name);
    console.log('   Target:', target.name);
}

// ============================================
// EXAMPLE 2: Create Multiple Attack Arrows
// ============================================
function example2_MultipleAttackArrows() {
    console.log('📋 Example 2: Creating multiple attack arrows');
    
    const tokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    if (tokens.length < 3) {
        console.error('Need at least 3 tokens on the map');
        return;
    }
    
    // Create multiple attacks from one attacker
    const attacker = tokens[0].tokenData;
    const createdAttacks = [];
    
    for (let i = 1; i < Math.min(tokens.length, 4); i++) {
        const target = tokens[i].tokenData;
        const attackId = window.attackVisualizationManager.addAttackLine(attacker, target);
        
        if (attackId) {
            createdAttacks.push({
                id: attackId,
                target: target.name
            });
        }
    }
    
    console.log(`✅ Created ${createdAttacks.length} attack arrows from ${attacker.name}`);
    createdAttacks.forEach(attack => {
        console.log(`   → ${attack.target} (${attack.id})`);
    });
}

// ============================================
// EXAMPLE 3: Create Attack with Custom Type
// ============================================
function example3_CustomAttackType() {
    console.log('📋 Example 3: Creating attack with custom type');
    
    const tokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    if (tokens.length < 2) {
        console.error('Need at least 2 tokens on the map');
        return;
    }
    
    const attacker = tokens[0].tokenData;
    const target = tokens[1].tokenData;
    
    // Create attack order with custom type
    const attackOrder = {
        intent: {
            natoAttackType: 'flanking',  // Options: frontal, flanking, envelopment, penetration, raid, ambush
            attackType: 'Flanking Maneuver',
            maneuverForm: 'Mobile',
            desiredEffect: 'Disrupt Enemy Flank'
        },
        timing: {
            startTurn: 5,
            durationTurns: 3
        }
    };
    
    const attackId = window.attackVisualizationManager.addAttackLine(attacker, target, attackOrder);
    
    console.log('✅ Flanking attack created:', attackId);
    console.log('   Type:', attackOrder.intent.natoAttackType);
    console.log('   Attacker:', attacker.name);
    console.log('   Target:', target.name);
}

// ============================================
// EXAMPLE 4: Create Different Attack Types
// ============================================
function example4_DifferentAttackTypes() {
    console.log('📋 Example 4: Creating different NATO attack types');
    
    const tokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    if (tokens.length < 5) {
        console.error('Need at least 5 tokens on the map for this example');
        return;
    }
    
    const attackTypes = [
        { type: 'frontal', name: 'Frontal Assault' },
        { type: 'flanking', name: 'Flanking Maneuver' },
        { type: 'envelopment', name: 'Envelopment' },
        { type: 'penetration', name: 'Penetration' },
        { type: 'raid', name: 'Raid' }
    ];
    
    const attacker = tokens[0].tokenData;
    
    attackTypes.forEach((attackType, index) => {
        if (index + 1 < tokens.length) {
            const target = tokens[index + 1].tokenData;
            
            const attackOrder = {
                intent: {
                    natoAttackType: attackType.type,
                    attackType: attackType.name
                }
            };
            
            const attackId = window.attackVisualizationManager.addAttackLine(attacker, target, attackOrder);
            
            console.log(`✅ ${attackType.name} created: ${attacker.name} → ${target.name}`);
        }
    });
}

// ============================================
// EXAMPLE 5: Remove Attack Arrows
// ============================================
function example5_RemoveAttackArrows() {
    console.log('📋 Example 5: Removing attack arrows');
    
    // Get all current attack lines
    const allAttacks = window.attackVisualizationManager.getAllAttackLines();
    
    console.log(`Found ${allAttacks.length} attack arrows`);
    
    if (allAttacks.length === 0) {
        console.log('No attack arrows to remove');
        return;
    }
    
    // Remove first attack
    const firstAttack = allAttacks[0];
    const attackId = firstAttack.id;
    
    window.attackVisualizationManager.removeAttackLine(attackId);
    
    console.log(`✅ Removed attack arrow: ${firstAttack.attacker.name} → ${firstAttack.target.name}`);
}

// ============================================
// EXAMPLE 6: Clear All Attack Arrows
// ============================================
function example6_ClearAllAttackArrows() {
    console.log('📋 Example 6: Clearing all attack arrows');
    
    const allAttacks = window.attackVisualizationManager.getAllAttackLines();
    console.log(`Clearing ${allAttacks.length} attack arrows...`);
    
    window.attackVisualizationManager.clearAllAttackLines();
    
    console.log('✅ All attack arrows cleared');
}

// ============================================
// EXAMPLE 7: Load Attacks from Database
// ============================================
async function example7_LoadFromDatabase() {
    console.log('📋 Example 7: Loading attack orders from database');
    
    await window.attackVisualizationManager.loadAttackOrdersFromDatabase();
    
    const allAttacks = window.attackVisualizationManager.getAllAttackLines();
    console.log(`✅ Loaded ${allAttacks.length} attack arrows from database`);
}

// ============================================
// EXAMPLE 8: Get Token-Specific Attacks
// ============================================
function example8_GetTokenAttacks() {
    console.log('📋 Example 8: Getting attacks for specific token');
    
    const tokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    if (tokens.length === 0) {
        console.error('No tokens on the map');
        return;
    }
    
    const token = tokens[0].tokenData;
    const tokenAttacks = window.attackVisualizationManager.showTokenAttacks(token.id);
    
    console.log(`Token: ${token.name}`);
    console.log(`Involved in ${tokenAttacks.length} attacks:`);
    
    tokenAttacks.forEach(attack => {
        const role = attack.attacker.id === token.id ? 'Attacker' : 'Target';
        const other = attack.attacker.id === token.id ? attack.target.name : attack.attacker.name;
        console.log(`   ${role} against ${other}`);
    });
}

// ============================================
// EXAMPLE 9: Test All Features
// ============================================
async function example9_TestAllFeatures() {
    console.log('🧪 Testing all NATO attack arrow features...\n');
    
    // Clear existing attacks
    console.log('1️⃣ Clearing existing attacks...');
    window.attackVisualizationManager.clearAllAttackLines();
    
    // Wait a moment
    await new Promise(resolve => setTimeout(resolve, 500));
    
    // Create simple attack
    console.log('\n2️⃣ Creating simple attack...');
    example1_SimpleAttackArrow();
    
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Create custom type attack
    console.log('\n3️⃣ Creating custom type attack...');
    example3_CustomAttackType();
    
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Show all attacks
    const allAttacks = window.attackVisualizationManager.getAllAttackLines();
    console.log(`\n✅ Test complete! Created ${allAttacks.length} attack arrows`);
    
    console.log('\n📊 Attack Summary:');
    allAttacks.forEach((attack, index) => {
        console.log(`   ${index + 1}. ${attack.attacker.name} → ${attack.target.name}`);
    });
}

// ============================================
// EXAMPLE 10: Interactive Demo
// ============================================
function example10_InteractiveDemo() {
    console.log('🎮 Interactive NATO Attack Arrow Demo');
    console.log('═══════════════════════════════════════════════\n');
    console.log('Available Commands:');
    console.log('  example1_SimpleAttackArrow()      - Create simple attack');
    console.log('  example2_MultipleAttackArrows()   - Create multiple attacks');
    console.log('  example3_CustomAttackType()       - Create flanking attack');
    console.log('  example4_DifferentAttackTypes()   - Create all attack types');
    console.log('  example5_RemoveAttackArrows()     - Remove one attack');
    console.log('  example6_ClearAllAttackArrows()   - Clear all attacks');
    console.log('  example7_LoadFromDatabase()       - Load from database');
    console.log('  example8_GetTokenAttacks()        - Get token-specific attacks');
    console.log('  example9_TestAllFeatures()        - Test all features\n');
    console.log('═══════════════════════════════════════════════');
}

// ============================================
// Utility Functions
// ============================================

/**
 * Create a coordinated attack from multiple tokens to one target
 */
function createCoordinatedAttack(attackerTokenIds, targetTokenId) {
    console.log('🎯 Creating coordinated attack...');
    
    const allTokens = window.attackVisualizationManager.getAllTokenMarkers();
    const createdAttacks = [];
    
    attackerTokenIds.forEach(attackerId => {
        const attacker = allTokens.find(t => t.tokenId === attackerId);
        const target = allTokens.find(t => t.tokenId === targetTokenId);
        
        if (attacker && target) {
            const attackId = window.attackVisualizationManager.addAttackLine(
                attacker.tokenData,
                target.tokenData
            );
            
            if (attackId) {
                createdAttacks.push(attackId);
                console.log(`✅ ${attacker.tokenData.name} → ${target.tokenData.name}`);
            }
        }
    });
    
    console.log(`✅ Coordinated attack created with ${createdAttacks.length} attacking units`);
    return createdAttacks;
}

/**
 * Create attack chain (A→B→C→D)
 */
function createAttackChain(tokenIds) {
    console.log('⛓️ Creating attack chain...');
    
    const allTokens = window.attackVisualizationManager.getAllTokenMarkers();
    const createdAttacks = [];
    
    for (let i = 0; i < tokenIds.length - 1; i++) {
        const attacker = allTokens.find(t => t.tokenId === tokenIds[i]);
        const target = allTokens.find(t => t.tokenId === tokenIds[i + 1]);
        
        if (attacker && target) {
            const attackId = window.attackVisualizationManager.addAttackLine(
                attacker.tokenData,
                target.tokenData
            );
            
            if (attackId) {
                createdAttacks.push(attackId);
                console.log(`✅ ${attacker.tokenData.name} → ${target.tokenData.name}`);
            }
        }
    }
    
    console.log(`✅ Attack chain created with ${createdAttacks.length} links`);
    return createdAttacks;
}

/**
 * Show attack statistics
 */
function showAttackStatistics() {
    const allAttacks = window.attackVisualizationManager.getAllAttackLines();
    
    console.log('📊 Attack Statistics');
    console.log('═══════════════════════════════════════════════');
    console.log(`Total Attacks: ${allAttacks.length}`);
    
    // Group by attacker
    const attacksByAttacker = {};
    allAttacks.forEach(attack => {
        const attackerName = attack.attacker.name;
        attacksByAttacker[attackerName] = (attacksByAttacker[attackerName] || 0) + 1;
    });
    
    console.log('\nAttacks by Attacker:');
    Object.entries(attacksByAttacker).forEach(([name, count]) => {
        console.log(`  ${name}: ${count} attack(s)`);
    });
    
    // Group by target
    const attacksByTarget = {};
    allAttacks.forEach(attack => {
        const targetName = attack.target.name;
        attacksByTarget[targetName] = (attacksByTarget[targetName] || 0) + 1;
    });
    
    console.log('\nAttacks by Target:');
    Object.entries(attacksByTarget).forEach(([name, count]) => {
        console.log(`  ${name}: ${count} attack(s) incoming`);
    });
    
    console.log('═══════════════════════════════════════════════');
}

// ============================================
// Export Functions to Window
// ============================================
window.natoAttackExamples = {
    // Examples
    example1_SimpleAttackArrow,
    example2_MultipleAttackArrows,
    example3_CustomAttackType,
    example4_DifferentAttackTypes,
    example5_RemoveAttackArrows,
    example6_ClearAllAttackArrows,
    example7_LoadFromDatabase,
    example8_GetTokenAttacks,
    example9_TestAllFeatures,
    example10_InteractiveDemo,
    
    // Utilities
    createCoordinatedAttack,
    createAttackChain,
    showAttackStatistics
};

// ============================================
// Auto-run Demo on Load
// ============================================
console.log('🎯 NATO Attack Arrows - Quick Start Loaded!');
console.log('Run example10_InteractiveDemo() to see all available commands');
console.log('Or try: natoAttackExamples.example9_TestAllFeatures()');

