/**
 * Test Attack Arrow Implementation
 * Scenario: Blue force token "10" attacking suspected token "35"
 */

// Test function to verify attack arrow display
window.testAttackArrowWithScenario = function() {
    console.log('🧪 Testing Attack Arrow Scenario');
    console.log('═══════════════════════════════════════════════');
    console.log('Scenario: Blue Force "10" attacking Suspected Token "35"');
    console.log('Expected: Arrowhead shows "10" (attacker symbol)');
    console.log('═══════════════════════════════════════════════\n');
    
    // Get all tokens on map
    const allTokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    console.log('📋 Available tokens on map:');
    allTokens.forEach((token, index) => {
        console.log(`  ${index + 1}. ${token.tokenData.name} (ID: ${token.tokenId})`);
    });
    
    if (allTokens.length < 2) {
        console.error('❌ Need at least 2 tokens on map');
        console.log('\n💡 Tip: Place tokens on the map first, then run this test');
        return;
    }
    
    // Find attacker (look for token with "10" in name or use first token)
    let attackerToken = allTokens.find(t => t.tokenData.name.includes('10'));
    if (!attackerToken) {
        attackerToken = allTokens[0];
        console.log(`\n⚠️ No token with "10" found, using: ${attackerToken.tokenData.name}`);
    } else {
        console.log(`\n✅ Found attacker token: ${attackerToken.tokenData.name}`);
    }
    
    // Find target (look for token with "35" in name or use second token)
    let targetToken = allTokens.find(t => t.tokenData.name.includes('35'));
    if (!targetToken) {
        targetToken = allTokens[1];
        console.log(`⚠️ No token with "35" found, using: ${targetToken.tokenData.name}`);
    } else {
        console.log(`✅ Found target token: ${targetToken.tokenData.name}`);
    }
    
    // Extract symbol numbers for verification
    const attackerNumber = extractTestSymbolNumber(attackerToken.tokenData.name);
    const targetNumber = extractTestSymbolNumber(targetToken.tokenData.name);
    
    console.log('\n📊 Symbol Numbers:');
    console.log(`  Attacker: "${attackerToken.tokenData.name}" → Symbol: "${attackerNumber}"`);
    console.log(`  Target: "${targetToken.tokenData.name}" → Symbol: "${targetNumber}"`);
    
    // Create attack arrow
    console.log('\n🎯 Creating attack arrow...');
    const attackId = window.attackVisualizationManager.addAttackLine(
        attackerToken.tokenData,
        targetToken.tokenData
    );
    
    if (attackId) {
        console.log('✅ Attack arrow created successfully!');
        console.log(`   Attack ID: ${attackId}`);
        console.log(`   Arrow direction: ${attackerToken.tokenData.name} → ${targetToken.tokenData.name}`);
        console.log(`   Arrowhead displays: "${attackerNumber}" (attacker's symbol)`);
        console.log('\n👀 Look at the map: The arrowhead should show "${attackerNumber}" inside it');
    } else {
        console.error('❌ Failed to create attack arrow');
    }
    
    console.log('\n═══════════════════════════════════════════════');
};

// Helper function to extract symbol number (same logic as in AttackSymbolRenderer)
function extractTestSymbolNumber(name) {
    if (!name) return '';
    
    const patterns = [
        /(\d{1,3})$/,           // Numbers at end
        /\s(\d{1,3})\s/,        // Numbers in middle
        /[A-Za-z]\s*(\d{1,3})/, // Letter followed by number
    ];
    
    for (const pattern of patterns) {
        const match = name.match(pattern);
        if (match && match[1]) {
            return match[1];
        }
    }
    
    return name.substring(0, 3).toUpperCase();
}

// Test function to create the exact scenario from the image
window.testExactScenario = function() {
    console.log('🎯 Creating Exact Scenario: Blue "10" → Suspected "35"');
    console.log('═══════════════════════════════════════════════\n');
    
    // You'll need to replace these with actual token IDs from your map
    // Run window.attackVisualizationManager.getAllTokenMarkers() first to get IDs
    
    const allTokens = window.attackVisualizationManager.getAllTokenMarkers();
    
    // Show instructions to user
    console.log('📝 Instructions:');
    console.log('1. Find your token IDs by looking at the list above');
    console.log('2. Look for the token representing unit "10" (attacker)');
    console.log('3. Look for the token representing unit "35" (target)');
    console.log('4. The arrow will show "10" in the arrowhead\n');
    
    // List all available tokens
    console.log('Available tokens:');
    allTokens.forEach((token, index) => {
        console.log(`  [${index}] ${token.tokenData.name}`);
        console.log(`      ID: ${token.tokenId}`);
        console.log(`      Symbol: "${extractTestSymbolNumber(token.tokenData.name)}"`);
    });
    
    console.log('\n💡 Run: testAttackArrowWithScenario() to automatically test with available tokens');
};

// Quick verification function
window.verifyArrowheadSymbol = function(attackId) {
    console.log('🔍 Verifying arrowhead symbol...\n');
    
    const attackLineData = window.attackVisualizationManager.attackLines.get(attackId);
    
    if (!attackLineData) {
        console.error(`❌ Attack line not found: ${attackId}`);
        return;
    }
    
    const attackerName = attackLineData.attacker.name;
    const targetName = attackLineData.target.name;
    const attackerSymbol = extractTestSymbolNumber(attackerName);
    
    console.log('Attack Line Details:');
    console.log(`  Attacker: ${attackerName}`);
    console.log(`  Target: ${targetName}`);
    console.log(`  Attacker Symbol: "${attackerSymbol}"`);
    console.log(`\n✅ The arrowhead should display: "${attackerSymbol}"`);
    console.log(`   (Not "${extractTestSymbolNumber(targetName)}" from the target)`);
};

// Show all current attacks
window.showCurrentAttacks = function() {
    console.log('📊 Current Attack Arrows on Map');
    console.log('═══════════════════════════════════════════════\n');
    
    const allAttacks = window.attackVisualizationManager.getAllAttackLines();
    
    if (allAttacks.length === 0) {
        console.log('No attack arrows currently on the map');
        return;
    }
    
    allAttacks.forEach((attack, index) => {
        const attackerSymbol = extractTestSymbolNumber(attack.attacker.name);
        const targetSymbol = extractTestSymbolNumber(attack.target.name);
        
        console.log(`${index + 1}. ${attack.attacker.name} → ${attack.target.name}`);
        console.log(`   Attacker Symbol: "${attackerSymbol}" (shown in arrowhead)`);
        console.log(`   Target Symbol: "${targetSymbol}" (being attacked)`);
        console.log(`   Attack ID: ${attack.id}\n`);
    });
    
    console.log('═══════════════════════════════════════════════');
};

console.log('🎯 Attack Arrow Testing Loaded!');
console.log('\nAvailable test commands:');
console.log('  testAttackArrowWithScenario()  - Test with available tokens');
console.log('  testExactScenario()            - Show exact scenario setup');
console.log('  showCurrentAttacks()           - Show all current attacks');
console.log('  verifyArrowheadSymbol(id)      - Verify arrowhead shows correct symbol');

