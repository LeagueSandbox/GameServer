function finishCasting()
    local speedIncrease = 12 + spellLevel * 4
    local buff = Buff.new("Overdrive", 8.0, BUFFTYPE_TEMPORARY, owner)
    
    print("Speed increase" ..speedIncrease)
    
    buff:setMovementSpeedPercentModifier(speedIncrease)
    addBuff(buff, owner)
end

function applyEffects()
end
