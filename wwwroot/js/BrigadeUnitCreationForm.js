/**
 * Brigade Unit Creation Form JavaScript
 * Handles create/edit operations for military units within a brigade
 */

// Load existing unit data into form (for edit mode)
$(document).ready(function() {
    const existingUnitJson = document.getElementById('brigadeExistingUnitJson')?.value;
    
    if (existingUnitJson && existingUnitJson.trim() !== '') {
        try {
            const unitData = JSON.parse(existingUnitJson);
            console.log('📝 Loading existing brigade unit data for editing:', unitData);
            
            const unitType = document.getElementById('brigadeUnitType')?.value;
            
            // Populate form based on unit type
            if (unitType === 'infantry') {
                populateBrigadeInfantryForm(unitData);
            } else if (unitType === 'armoured') {
                populateBrigadeArmouredForm(unitData);
            } else if (unitType === 'artillery') {
                populateBrigadeArtilleryForm(unitData);
            } else if (unitType === 'logistics') {
                populateBrigadeLogisticsForm(unitData);
            } else if (unitType === 'engineering') {
                populateBrigadeEngineeringForm(unitData);
            }
        } catch (e) {
            console.error('Error parsing existing brigade unit data:', e);
        }
    }
});

// Populate Infantry form
function populateBrigadeInfantryForm(data) {
    $('#brigadeInfantryName').val(data.Name || '');
    $('#brigadeInfantryCode').val(data.UnitCode || '');
    $('#brigadeInfantryStrength').val(data.Strength || 0);
    $('#brigadeInfantryCompanies').val(data.Companies || 0);
    $('#brigadeInfantryDescription').val(data.Description || '');
    $('#brigadeInfantryATGMS').val(data.ATGMS || 0);
    $('#brigadeInfantryRocketLauncher').val(data.RocketLauncher || 0);
    $('#brigadeInfantryMortars81mm').val(data.Mortars81mm || 0);
    $('#brigadeInfantryMortars120mm').val(data.Mortars120mm || 0);
}

// Populate Armoured form
function populateBrigadeArmouredForm(data) {
    $('#brigadeArmouredName').val(data.Name || '');
    $('#brigadeArmouredCode').val(data.UnitCode || '');
    $('#brigadeArmouredStrength').val(data.Strength || 0);
    $('#brigadeArmouredSquadrons').val(data.Squadrons || 0);
    $('#brigadeArmouredTanks').val(data.Tanks || 0);
    $('#brigadeArmouredDescription').val(data.Description || '');
    $('#brigadeArmouredATGMS').val(data.ATGMS || 0);
    $('#brigadeArmouredMortars120mm').val(data.Mortars120mm || 0);
}

// Populate Artillery form
function populateBrigadeArtilleryForm(data) {
    $('#brigadeArtilleryName').val(data.Name || '');
    $('#brigadeArtilleryCode').val(data.UnitCode || '');
    $('#brigadeArtilleryStrength').val(data.Strength || 0);
    $('#brigadeArtilleryBatteries').val(data.Batteries || 0);
    $('#brigadeArtilleryGuns').val(data.Guns || 0);
    $('#brigadeArtilleryGunCaliber').val(data.GunCaliber || '');
    $('#brigadeArtilleryGunRange').val(data.GunRange || 0);
    $('#brigadeArtilleryDescription').val(data.Description || '');
}

// Populate Logistics form
function populateBrigadeLogisticsForm(data) {
    $('#brigadeLogisticsName').val(data.Name || '');
    $('#brigadeLogisticsCode').val(data.UnitCode || '');
    $('#brigadeLogisticsStrength').val(data.Strength || 0);
    $('#brigadeLogisticsCompanies').val(data.Companies || 0);
    $('#brigadeLogisticsDescription').val(data.Description || '');
    $('#brigadeLogisticsSupplyTrucks').val(data.SupplyTrucks || 0);
    $('#brigadeLogisticsFuelTrucks').val(data.FuelTrucks || 0);
    $('#brigadeLogisticsMaintenanceVehicles').val(data.MaintenanceVehicles || 0);
    $('#brigadeLogisticsRecoveryVehicles').val(data.RecoveryVehicles || 0);
}

// Populate Engineering form
function populateBrigadeEngineeringForm(data) {
    $('#brigadeEngineeringName').val(data.Name || '');
    $('#brigadeEngineeringCode').val(data.UnitCode || '');
    $('#brigadeEngineeringStrength').val(data.Strength || 0);
    $('#brigadeEngineeringPlatoons').val(data.Platoons || 0);
    $('#brigadeEngineeringDescription').val(data.Description || '');
    $('#brigadeEngineeringBulldozers').val(data.Bulldozers || 0);
    $('#brigadeEngineeringExcavators').val(data.Excavators || 0);
    $('#brigadeEngineeringMinePlows').val(data.MinePlows || 0);
    $('#brigadeEngineeringBridgeLayers').val(data.BridgeLayers || 0);
}

// Save brigade unit (main router function)
function saveBrigadeUnit() {
    const unitType = document.getElementById('brigadeUnitType')?.value;
    
    console.log('💾 Saving brigade unit of type:', unitType);
    
    // Route to appropriate save function
    if (unitType === 'infantry') {
        saveBrigadeInfantryBattalion();
    } else if (unitType === 'armoured') {
        saveBrigadeArmouredRegiment();
    } else if (unitType === 'artillery') {
        saveBrigadeArtilleryRegiment();
    } else if (unitType === 'logistics') {
        saveBrigadeLogisticsUnit();
    } else if (unitType === 'engineering') {
        saveBrigadeEngineeringCompany();
    } else {
        alert('Unknown unit type: ' + unitType);
    }
}

// Save Infantry Battalion
function saveBrigadeInfantryBattalion() {
    const tokenId = $('#brigadeTokenId').val();
    const brigadeId = $('#brigadeBrigadeId').val();
    const teamId = $('#brigadeTeamId').val();
    const forceType = $('#brigadeForceType').val();
    const unitId = $('#brigadeUnitId').val();
    
    const isEditMode = unitId && unitId.trim() !== '';
    
    const unitData = {
        Name: $('#brigadeInfantryName').val(),
        UnitCode: $('#brigadeInfantryCode').val(),
        Strength: parseInt($('#brigadeInfantryStrength').val()) || 0,
        Companies: parseInt($('#brigadeInfantryCompanies').val()) || 0,
        Description: $('#brigadeInfantryDescription').val() || '',
        ATGMS: parseInt($('#brigadeInfantryATGMS').val()) || 0,
        RocketLauncher: parseInt($('#brigadeInfantryRocketLauncher').val()) || 0,
        Mortars81mm: parseInt($('#brigadeInfantryMortars81mm').val()) || 0,
        Mortars120mm: parseInt($('#brigadeInfantryMortars120mm').val()) || 0,
        TokenId: tokenId,
        BrigadeId: brigadeId,
        TeamId: teamId,
        ForceType: forceType
    };
    
    if (isEditMode) {
        unitData.Id = unitId;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateTokenInfantryBattalion' : '/DataManagement/CreateTokenInfantryBattalion';
    
    saveBrigadeUnitToServer(endpoint, unitData, 'Infantry Battalion', tokenId, brigadeId);
}

// Save Armoured Regiment
function saveBrigadeArmouredRegiment() {
    const tokenId = $('#brigadeTokenId').val();
    const brigadeId = $('#brigadeBrigadeId').val();
    const teamId = $('#brigadeTeamId').val();
    const forceType = $('#brigadeForceType').val();
    const unitId = $('#brigadeUnitId').val();
    
    const isEditMode = unitId && unitId.trim() !== '';
    
    const unitData = {
        Name: $('#brigadeArmouredName').val(),
        UnitCode: $('#brigadeArmouredCode').val(),
        Strength: parseInt($('#brigadeArmouredStrength').val()) || 0,
        Squadrons: parseInt($('#brigadeArmouredSquadrons').val()) || 0,
        Tanks: parseInt($('#brigadeArmouredTanks').val()) || 0,
        Description: $('#brigadeArmouredDescription').val() || '',
        ATGMS: parseInt($('#brigadeArmouredATGMS').val()) || 0,
        Mortars120mm: parseInt($('#brigadeArmouredMortars120mm').val()) || 0,
        TokenId: tokenId,
        BrigadeId: brigadeId,
        TeamId: teamId,
        ForceType: forceType
    };
    
    if (isEditMode) {
        unitData.Id = unitId;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateTokenArmouredRegiment' : '/DataManagement/CreateTokenArmouredRegiment';
    
    saveBrigadeUnitToServer(endpoint, unitData, 'Armoured Regiment', tokenId, brigadeId);
}

// Save Artillery Regiment
function saveBrigadeArtilleryRegiment() {
    const tokenId = $('#brigadeTokenId').val();
    const brigadeId = $('#brigadeBrigadeId').val();
    const teamId = $('#brigadeTeamId').val();
    const forceType = $('#brigadeForceType').val();
    const unitId = $('#brigadeUnitId').val();
    
    const isEditMode = unitId && unitId.trim() !== '';
    
    const unitData = {
        Name: $('#brigadeArtilleryName').val(),
        UnitCode: $('#brigadeArtilleryCode').val(),
        Strength: parseInt($('#brigadeArtilleryStrength').val()) || 0,
        Batteries: parseInt($('#brigadeArtilleryBatteries').val()) || 0,
        Guns: parseInt($('#brigadeArtilleryGuns').val()) || 0,
        GunCaliber: $('#brigadeArtilleryGunCaliber').val() || '',
        GunRange: parseInt($('#brigadeArtilleryGunRange').val()) || 0,
        Description: $('#brigadeArtilleryDescription').val() || '',
        TokenId: tokenId,
        BrigadeId: brigadeId,
        TeamId: teamId,
        ForceType: forceType
    };
    
    if (isEditMode) {
        unitData.Id = unitId;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateTokenArtilleryRegiment' : '/DataManagement/CreateTokenArtilleryRegiment';
    
    saveBrigadeUnitToServer(endpoint, unitData, 'Artillery Regiment', tokenId, brigadeId);
}

// Save Logistics Unit
function saveBrigadeLogisticsUnit() {
    const tokenId = $('#brigadeTokenId').val();
    const brigadeId = $('#brigadeBrigadeId').val();
    const teamId = $('#brigadeTeamId').val();
    const forceType = $('#brigadeForceType').val();
    const unitId = $('#brigadeUnitId').val();
    
    const isEditMode = unitId && unitId.trim() !== '';
    
    const unitData = {
        Name: $('#brigadeLogisticsName').val(),
        UnitCode: $('#brigadeLogisticsCode').val(),
        Strength: parseInt($('#brigadeLogisticsStrength').val()) || 0,
        Companies: parseInt($('#brigadeLogisticsCompanies').val()) || 0,
        Description: $('#brigadeLogisticsDescription').val() || '',
        SupplyTrucks: parseInt($('#brigadeLogisticsSupplyTrucks').val()) || 0,
        FuelTrucks: parseInt($('#brigadeLogisticsFuelTrucks').val()) || 0,
        MaintenanceVehicles: parseInt($('#brigadeLogisticsMaintenanceVehicles').val()) || 0,
        RecoveryVehicles: parseInt($('#brigadeLogisticsRecoveryVehicles').val()) || 0,
        TokenId: tokenId,
        BrigadeId: brigadeId,
        TeamId: teamId,
        ForceType: forceType
    };
    
    if (isEditMode) {
        unitData.Id = unitId;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateLogisticsUnit' : '/DataManagement/CreateLogisticsUnit';
    
    saveBrigadeUnitToServer(endpoint, unitData, 'Logistics Unit', tokenId, brigadeId);
}

// Save Engineering Company
function saveBrigadeEngineeringCompany() {
    const tokenId = $('#brigadeTokenId').val();
    const brigadeId = $('#brigadeBrigadeId').val();
    const teamId = $('#brigadeTeamId').val();
    const forceType = $('#brigadeForceType').val();
    const unitId = $('#brigadeUnitId').val();
    
    const isEditMode = unitId && unitId.trim() !== '';
    
    const unitData = {
        Name: $('#brigadeEngineeringName').val(),
        UnitCode: $('#brigadeEngineeringCode').val(),
        Strength: parseInt($('#brigadeEngineeringStrength').val()) || 0,
        Platoons: parseInt($('#brigadeEngineeringPlatoons').val()) || 0,
        Description: $('#brigadeEngineeringDescription').val() || '',
        Bulldozers: parseInt($('#brigadeEngineeringBulldozers').val()) || 0,
        Excavators: parseInt($('#brigadeEngineeringExcavators').val()) || 0,
        MinePlows: parseInt($('#brigadeEngineeringMinePlows').val()) || 0,
        BridgeLayers: parseInt($('#brigadeEngineeringBridgeLayers').val()) || 0,
        TokenId: tokenId,
        BrigadeId: brigadeId,
        TeamId: teamId,
        ForceType: forceType
    };
    
    if (isEditMode) {
        unitData.Id = unitId;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateCombatEngineeringCompany' : '/DataManagement/CreateCombatEngineeringCompany';
    
    saveBrigadeUnitToServer(endpoint, unitData, 'Engineering Company', tokenId, brigadeId);
}

// Generic save function for all brigade units
function saveBrigadeUnitToServer(endpoint, data, unitName, tokenId, brigadeId) {
    console.log(`💾 Saving ${unitName} to:`, endpoint);
    console.log('Data:', data);
    
    // Determine HTTP method based on whether it's an update or create
    const isUpdate = data.Id && data.Id.trim() !== '';
    const httpMethod = isUpdate ? 'PUT' : 'POST';
    
    console.log(`🔧 Using HTTP method: ${httpMethod} (isUpdate: ${isUpdate})`);
    
    // Show loader
    showLoader();
    
    $.ajax({
        url: endpoint,
        type: httpMethod,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            console.log(`✅ ${unitName} saved successfully - no page refresh`);
            
            // Close the creation modal
            $('#brigadeUnitCreationModal').modal('hide');
            
            // Reload the brigade units list (modal only)
            reloadBrigadeUnitsList(tokenId, brigadeId);
        },
        error: function(xhr, status, error) {
            hideLoader();
            console.error(`❌ Error saving ${unitName}:`, error);
            console.error('Response:', xhr.responseText);
            alert(`Failed to save ${unitName}: ` + error);
        }
    });
}

// Reload the brigade units list modal
function reloadBrigadeUnitsList(tokenId, brigadeId) {
    console.log('🔄 Reloading brigade units list');
    
    $.get('/DataManagement/UnitsDataEntryForm', { tokenId: tokenId, brigadeId: brigadeId }, function(data) {
        // Replace the entire modal content
        $('#brigadeUnitModal .modal-content').html($(data).find('.modal-content').html());
        
        // Show the modal again
        if (!$('#brigadeUnitModal').hasClass('show')) {
            $('#brigadeUnitModal').modal('show');
        }
        
        // Hide loader
        hideLoader();
    }).fail(function(xhr, status, error) {
        hideLoader();
        console.error('❌ Error reloading brigade units list:', error);
        alert('Failed to reload list: ' + error);
    });
}

