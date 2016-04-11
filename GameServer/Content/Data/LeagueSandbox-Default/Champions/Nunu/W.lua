function finishCasting()
    local speedIncrease = 7 + getSpellLevel()
    
    addBuff("BloodBoil", 12.0, getOwner())
end

function applyEffects()
end
