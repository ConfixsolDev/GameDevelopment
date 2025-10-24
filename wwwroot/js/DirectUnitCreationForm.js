/**
 * Direct Unit Creation Form JavaScript
 * Handles create/edit/delete operations for military units without brigade requirement
 */

// Load existing unit data into form (for edit mode)
$(document).ready(function() {
    const existingUnitJson = document.getElementById('directExistingUnitJson')?.value;
    
    if (existingUnitJson && existingUnitJson.trim() !== '') {
        try {
            const unitData = JSON.parse(existingUnitJson);
            console.log('📝 Loading existing unit data for editing:', unitData);
            
            const unitType = document.getElementById('directUnitType')?.value;
            
            // Populate form based on unit type
            if (unitType === 'infantry') {
                populateInfantryForm(unitData);
            } else if (unitType === 'armoured') {
                populateArmouredForm(unitData);
            } else if (unitType === 'artillery') {
                populateArtilleryForm(unitData);
            } else if (unitType === 'logistics') {
                populateLogisticsForm(unitData);
            } else if (unitType === 'engineering') {
                populateEngineeringForm(unitData);
            }
        } catch (e) {
            console.error('Error parsing existing unit data:', e);
        }
    }
});

// Populate Infantry form
function populateInfantryForm(data) {
    $('#directInfantryName').val(data.Name || '');
    $('#directInfantryCode').val(data.UnitCode || '');
    $('#directInfantryStrength').val(data.Strength || 0);
    $('#directInfantryCompanies').val(data.Companies || 0);
    $('#directInfantryDescription').val(data.Description || '');
    $('#directInfantryATGMS').val(data.ATGMS || 0);
    $('#directInfantryRocketLauncher').val(data.RocketLauncher || 0);
    $('#directInfantryMortars81mm').val(data.Mortars81mm || 0);
    $('#directInfantryMortars120mm').val(data.Mortars120mm || 0);
    $('#directInfantryGrenadeLaunchers').val(data.GrenadeLaunchers || 0);
    $('#directInfantryHMG_AGL').val(data.HMG_AGL || 0);
    $('#directInfantryMG_LMG').val(data.MG_LMG || 0);
    $('#directInfantryMANPADS').val(data.MANPADS || 0);
    $('#directInfantrySniper').val(data.Sniper || 0);
    $('#directInfantryDrones').val(data.Drones || 0);
    $('#directInfantryNVG').val(data.NVG || 0);
}

// Populate Armoured form
function populateArmouredForm(data) {
    $('#directArmouredName').val(data.Name || '');
    $('#directArmouredCode').val(data.UnitCode || '');
    $('#directArmouredStrength').val(data.Strength || 0);
    $('#directArmouredSquadrons').val(data.Squadrons || 0);
    $('#directArmouredTanks').val(data.Tanks || 0);
    $('#directArmouredDescription').val(data.Description || '');
    $('#directArmouredATGMS').val(data.ATGMS || 0);
    $('#directArmouredMortars120mm').val(data.Mortars120mm || 0);
    $('#directArmouredHMG').val(data.HMG || 0);
    $('#directArmouredDrones').val(data.Drones || 0);
    $('#directArmouredNVG').val(data.NVG || 0);
}

// Populate Artillery form
function populateArtilleryForm(data) {
    $('#directArtilleryName').val(data.Name || '');
    $('#directArtilleryCode').val(data.UnitCode || '');
    $('#directArtilleryStrength').val(data.Strength || 0);
    $('#directArtilleryBatteries').val(data.Batteries || 0);
    $('#directArtilleryGuns').val(data.Guns || 0);
    $('#directArtilleryGunCaliber').val(data.GunCaliber || '');
    $('#directArtilleryGunRange').val(data.GunRange || 0);
    $('#directArtilleryDescription').val(data.Description || '');
    $('#directArtilleryHMG').val(data.HMG || 0);
    $('#directArtilleryDrones').val(data.Drones || 0);
}

// Populate Logistics form
function populateLogisticsForm(data) {
    $('#directLogisticsName').val(data.Name || '');
    $('#directLogisticsCode').val(data.UnitCode || '');
    $('#directLogisticsStrength').val(data.Strength || 0);
    $('#directLogisticsCompanies').val(data.Companies || 0);
    $('#directLogisticsDescription').val(data.Description || '');
    $('#directLogisticsSupplyTrucks').val(data.SupplyTrucks || 0);
    $('#directLogisticsFuelTrucks').val(data.FuelTrucks || 0);
    $('#directLogisticsWaterTrucks').val(data.WaterTrucks || 0);
    $('#directLogisticsAmmoTrucks').val(data.AmmunitionTrucks || 0);
    $('#directLogisticsMaintenanceVehicles').val(data.MaintenanceVehicles || 0);
    $('#directLogisticsRecoveryVehicles').val(data.RecoveryVehicles || 0);
    $('#directLogisticsMobileWorkshops').val(data.MobileWorkshops || 0);
    $('#directLogisticsFuelCapacity').val(data.FuelCapacity || 0);
    $('#directLogisticsWaterCapacity').val(data.WaterCapacity || 0);
    $('#directLogisticsHMG').val(data.HMG || 0);
    $('#directLogisticsLMG').val(data.LMG || 0);
}

// Populate Engineering form
function populateEngineeringForm(data) {
    $('#directEngineeringName').val(data.Name || '');
    $('#directEngineeringCode').val(data.UnitCode || '');
    $('#directEngineeringStrength').val(data.Strength || 0);
    $('#directEngineeringPlatoons').val(data.Platoons || 0);
    $('#directEngineeringDescription').val(data.Description || '');
    $('#directEngineeringVehicles').val(data.EngineerVehicles || 0);
    $('#directEngineeringBridgeLayers').val(data.BridgeLayingVehicles || 0);
    $('#directEngineeringMineClearers').val(data.MineClearingVehicles || 0);
    $('#directEngineeringBulldozers').val(data.Bulldozers || 0);
    $('#directEngineeringExcavators').val(data.Excavators || 0);
    $('#directEngineeringCranes').val(data.Cranes || 0);
    $('#directEngineeringATGMS').val(data.ATGMS || 0);
    $('#directEngineeringHMG').val(data.HMG || 0);
    $('#directEngineeringLMG').val(data.LMG || 0);
}

// Close direct unit creation modal
function closeDirectUnitCreationModal() {
    console.log('🔒 Closing direct unit creation modal');
    $('#directUnitCreationModal').modal('hide');
}

// Save direct unit (without brigade)
function saveDirectUnit() {
    const unitType = document.getElementById('directUnitType').value;
    console.log('💾 Saving direct unit:', unitType);
    
    if (unitType === 'infantry') {
        saveDirectInfantryBattalion();
    } else if (unitType === 'armoured') {
        saveDirectArmouredRegiment();
    } else if (unitType === 'artillery') {
        saveDirectArtilleryRegiment();
    } else if (unitType === 'logistics') {
        saveDirectLogisticsUnit();
    } else if (unitType === 'engineering') {
        saveDirectEngineeringCompany();
    }
}

// Save Direct Infantry Battalion
function saveDirectInfantryBattalion() {
    console.log('💾 Saving Direct Infantry Battalion...');
    
    const unitId = document.getElementById('directUnitId').value;
    const isEditMode = unitId && unitId.trim() !== '';
    
    const infantryData = {
        Id: isEditMode ? unitId : null,
        BrigadeId: null,
        TokenId: document.getElementById('directTokenId').value,
        TeamId: document.getElementById('directTeamId').value,
        ForceType: document.getElementById('directForceType').value,
        Name: document.getElementById('directInfantryName').value,
        UnitCode: document.getElementById('directInfantryCode').value,
        Strength: parseInt(document.getElementById('directInfantryStrength').value) || 0,
        Companies: parseInt(document.getElementById('directInfantryCompanies').value) || 0,
        Description: document.getElementById('directInfantryDescription').value,
        ATGMS: parseInt(document.getElementById('directInfantryATGMS').value) || 0,
        RocketLauncher: parseInt(document.getElementById('directInfantryRocketLauncher').value) || 0,
        Mortars81mm: parseInt(document.getElementById('directInfantryMortars81mm').value) || 0,
        Mortars120mm: parseInt(document.getElementById('directInfantryMortars120mm').value) || 0,
        GrenadeLaunchers: parseInt(document.getElementById('directInfantryGrenadeLaunchers').value) || 0,
        HMG_AGL: parseInt(document.getElementById('directInfantryHMG_AGL').value) || 0,
        MG_LMG: parseInt(document.getElementById('directInfantryMG_LMG').value) || 0,
        MANPADS: parseInt(document.getElementById('directInfantryMANPADS').value) || 0,
        Sniper: parseInt(document.getElementById('directInfantrySniper').value) || 0,
        Drones: parseInt(document.getElementById('directInfantryDrones').value) || 0,
        NVG: parseInt(document.getElementById('directInfantryNVG').value) || 0,
        SupplyState: 100,
        StrengthPercentage: 100,
        IsActive: true
    };
    
    if (!infantryData.Name || !infantryData.UnitCode) {
        alert('Please fill in all required fields');
        return;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateTokenInfantryBattalion' : '/DataManagement/CreateTokenInfantryBattalion';
    saveUnitToServer(endpoint, infantryData, 'Infantry Battalion');
}

// Save Direct Armoured Regiment
function saveDirectArmouredRegiment() {
    console.log('💾 Saving Direct Armoured Regiment...');
    
    const unitId = document.getElementById('directUnitId').value;
    const isEditMode = unitId && unitId.trim() !== '';
    
    const armouredData = {
        Id: isEditMode ? unitId : null,
        BrigadeId: null,
        TokenId: document.getElementById('directTokenId').value,
        TeamId: document.getElementById('directTeamId').value,
        ForceType: document.getElementById('directForceType').value,
        Name: document.getElementById('directArmouredName').value,
        UnitCode: document.getElementById('directArmouredCode').value,
        Strength: parseInt(document.getElementById('directArmouredStrength').value) || 0,
        Squadrons: parseInt(document.getElementById('directArmouredSquadrons').value) || 0,
        Tanks: parseInt(document.getElementById('directArmouredTanks').value) || 0,
        Description: document.getElementById('directArmouredDescription').value,
        ATGMS: parseInt(document.getElementById('directArmouredATGMS').value) || 0,
        Mortars120mm: parseInt(document.getElementById('directArmouredMortars120mm').value) || 0,
        HMG: parseInt(document.getElementById('directArmouredHMG').value) || 0,
        Drones: parseInt(document.getElementById('directArmouredDrones').value) || 0,
        NVG: parseInt(document.getElementById('directArmouredNVG').value) || 0,
        SupplyState: 100,
        StrengthPercentage: 100,
        IsActive: true
    };
    
    if (!armouredData.Name || !armouredData.UnitCode) {
        alert('Please fill in all required fields');
        return;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateTokenArmouredRegiment' : '/DataManagement/CreateTokenArmouredRegiment';
    saveUnitToServer(endpoint, armouredData, 'Armoured Regiment');
}

// Save Direct Artillery Regiment
function saveDirectArtilleryRegiment() {
    console.log('💾 Saving Direct Artillery Regiment...');
    
    const unitId = document.getElementById('directUnitId').value;
    const isEditMode = unitId && unitId.trim() !== '';
    
    const artilleryData = {
        Id: isEditMode ? unitId : null,
        BrigadeId: null,
        TokenId: document.getElementById('directTokenId').value,
        TeamId: document.getElementById('directTeamId').value,
        ForceType: document.getElementById('directForceType').value,
        Name: document.getElementById('directArtilleryName').value,
        UnitCode: document.getElementById('directArtilleryCode').value,
        Strength: parseInt(document.getElementById('directArtilleryStrength').value) || 0,
        Batteries: parseInt(document.getElementById('directArtilleryBatteries').value) || 0,
        Guns: parseInt(document.getElementById('directArtilleryGuns').value) || 0,
        GunCaliber: document.getElementById('directArtilleryGunCaliber').value,
        GunRange: parseInt(document.getElementById('directArtilleryGunRange').value) || 0,
        Description: document.getElementById('directArtilleryDescription').value,
        HMG: parseInt(document.getElementById('directArtilleryHMG').value) || 0,
        Drones: parseInt(document.getElementById('directArtilleryDrones').value) || 0,
        SupplyState: 100,
        StrengthPercentage: 100,
        IsActive: true
    };
    
    if (!artilleryData.Name || !artilleryData.UnitCode || !artilleryData.GunCaliber) {
        alert('Please fill in all required fields');
        return;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateTokenArtilleryRegiment' : '/DataManagement/CreateTokenArtilleryRegiment';
    saveUnitToServer(endpoint, artilleryData, 'Artillery Regiment');
}

// Save Direct Logistics Unit
function saveDirectLogisticsUnit() {
    console.log('💾 Saving Direct Logistics Unit...');
    
    const unitId = document.getElementById('directUnitId').value;
    const isEditMode = unitId && unitId.trim() !== '';
    
    const logisticsData = {
        Id: isEditMode ? unitId : null,
        BrigadeId: null,
        TokenId: document.getElementById('directTokenId').value,
        TeamId: document.getElementById('directTeamId').value,
        ForceType: document.getElementById('directForceType').value,
        Name: document.getElementById('directLogisticsName').value,
        UnitCode: document.getElementById('directLogisticsCode').value,
        Strength: parseInt(document.getElementById('directLogisticsStrength').value) || 0,
        Companies: parseInt(document.getElementById('directLogisticsCompanies').value) || 0,
        Description: document.getElementById('directLogisticsDescription').value,
        SupplyTrucks: parseInt(document.getElementById('directLogisticsSupplyTrucks').value) || 0,
        FuelTrucks: parseInt(document.getElementById('directLogisticsFuelTrucks').value) || 0,
        WaterTrucks: parseInt(document.getElementById('directLogisticsWaterTrucks').value) || 0,
        AmmunitionTrucks: parseInt(document.getElementById('directLogisticsAmmoTrucks').value) || 0,
        MaintenanceVehicles: parseInt(document.getElementById('directLogisticsMaintenanceVehicles').value) || 0,
        RecoveryVehicles: parseInt(document.getElementById('directLogisticsRecoveryVehicles').value) || 0,
        MobileWorkshops: parseInt(document.getElementById('directLogisticsMobileWorkshops').value) || 0,
        FuelCapacity: parseInt(document.getElementById('directLogisticsFuelCapacity').value) || 0,
        WaterCapacity: parseInt(document.getElementById('directLogisticsWaterCapacity').value) || 0,
        HMG: parseInt(document.getElementById('directLogisticsHMG').value) || 0,
        LMG: parseInt(document.getElementById('directLogisticsLMG').value) || 0,
        SupplyState: 100,
        StrengthPercentage: 100,
        IsActive: true
    };
    
    if (!logisticsData.Name || !logisticsData.UnitCode) {
        alert('Please fill in all required fields');
        return;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateLogisticsUnit' : '/DataManagement/CreateLogisticsUnit';
    saveUnitToServer(endpoint, logisticsData, 'Logistics Unit');
}

// Save Direct Engineering Company
function saveDirectEngineeringCompany() {
    console.log('💾 Saving Direct Engineering Company...');
    
    const unitId = document.getElementById('directUnitId').value;
    const isEditMode = unitId && unitId.trim() !== '';
    
    const engineeringData = {
        Id: isEditMode ? unitId : null,
        BrigadeId: null,
        TokenId: document.getElementById('directTokenId').value,
        TeamId: document.getElementById('directTeamId').value,
        ForceType: document.getElementById('directForceType').value,
        Name: document.getElementById('directEngineeringName').value,
        UnitCode: document.getElementById('directEngineeringCode').value,
        Strength: parseInt(document.getElementById('directEngineeringStrength').value) || 0,
        Platoons: parseInt(document.getElementById('directEngineeringPlatoons').value) || 0,
        Description: document.getElementById('directEngineeringDescription').value,
        EngineerVehicles: parseInt(document.getElementById('directEngineeringVehicles').value) || 0,
        BridgeLayingVehicles: parseInt(document.getElementById('directEngineeringBridgeLayers').value) || 0,
        MineClearingVehicles: parseInt(document.getElementById('directEngineeringMineClearers').value) || 0,
        Bulldozers: parseInt(document.getElementById('directEngineeringBulldozers').value) || 0,
        Excavators: parseInt(document.getElementById('directEngineeringExcavators').value) || 0,
        Cranes: parseInt(document.getElementById('directEngineeringCranes').value) || 0,
        ATGMS: parseInt(document.getElementById('directEngineeringATGMS').value) || 0,
        HMG: parseInt(document.getElementById('directEngineeringHMG').value) || 0,
        LMG: parseInt(document.getElementById('directEngineeringLMG').value) || 0,
        SupplyState: 100,
        StrengthPercentage: 100,
        IsActive: true
    };
    
    if (!engineeringData.Name || !engineeringData.UnitCode) {
        alert('Please fill in all required fields');
        return;
    }
    
    const endpoint = isEditMode ? '/DataManagement/UpdateCombatEngineeringCompany' : '/DataManagement/CreateCombatEngineeringCompany';
    saveUnitToServer(endpoint, engineeringData, 'Combat Engineering Company');
}

// Generic save function
function saveUnitToServer(endpoint, unitData, unitTypeName) {
    $("#loading").show();
    
    const isEditMode = unitData.Id && unitData.Id.trim() !== '';
    const actionText = isEditMode ? 'updated' : 'created';
    
    $.ajax({
        url: endpoint,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(unitData),
        success: function(response) {
            console.log(`✅ ${unitTypeName} ${actionText} successfully`);
            
            // Close the creation modal
            closeDirectUnitCreationModal();
            
            // Show success message
            if (typeof toastr !== 'undefined') {
                toastr.success(`${unitTypeName} ${actionText} successfully`, 'Success');
            } else {
                alert(`${unitTypeName} ${actionText} successfully`);
            }
            
            // Reload the Direct Unit Form to show the new/updated unit
            setTimeout(() => {
                const unitType = document.getElementById('directUnitType').value;
                const tokenId = document.getElementById('directTokenId').value;
                
                // Reload the Direct Unit Form
                if (typeof openDirectUnitForm === 'function') {
                    openDirectUnitForm(unitType, tokenId);
                } else {
                    // Fallback: just close and hope for refresh
                    $('#directUnitModal').modal('hide');
                    if (typeof window.refreshTokenDataEntry === 'function') {
                        window.refreshTokenDataEntry();
                    }
                }
            }, 500);
        },
        error: function(xhr, status, error) {
            console.error(`❌ Error saving ${unitTypeName}:`, error);
            
            let errorMsg = `Failed to save ${unitTypeName}. Please try again.`;
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMsg = xhr.responseJSON.message;
            }
            
            if (typeof toastr !== 'undefined') {
                toastr.error(errorMsg, 'Error');
            } else {
                alert(errorMsg);
            }
        },
        complete: function() {
            $("#loading").hide();
        }
    });
}

