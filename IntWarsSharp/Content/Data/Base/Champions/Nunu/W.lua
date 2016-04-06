function finishCasting()
    local speedIncrease = 7 + getSpellLevel()
    local buff = Buff.new("ToxicShot", 12.0, BUFFTYPE_TEMPORARY, getOwner())
    
    print("Speed increase" .. speedIncrease)
    
    buff:setMovementSpeedPercentModifier(speedIncrease)
    addBuff(buff)
end

function applyEffects()
end
