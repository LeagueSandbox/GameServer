function finishCasting()
    local speedIncrease = 12 + getSpellLevel() * 8
    local buff = Buff.new("MoveQuick", 5.0, BUFFTYPE_TEMPORARY, getOwner())
    
    print("Speed increase" .. speedIncrease)
    
    buff:setMovementSpeedPercentModifier(speedIncrease)
    addBuff(buff, getOwner())
end

function applyEffects()
end
