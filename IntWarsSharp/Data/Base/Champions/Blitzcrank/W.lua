function finishCasting()
    local speedIncrease = 12 + getSpellLevel() * 4
    local buff = Buff.new("Overdrive", 8.0, BUFFTYPE_TEMPORARY, getOwner())
    
    print("Speed increase" ..speedIncrease)
    
    buff:setMovementSpeedPercentModifier(speedIncrease)
    addBuff(buff, getOwner())
end

function applyEffects()
end
