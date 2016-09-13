function onFinishCasting()
    local speedIncrease = 12 + spellLevel * 8
    local buff = Buff.new("MoveQuick", 5.0, 1, owner, owner)
    
    print("Speed increase" .. speedIncrease)
    
    buff:setMovementSpeedPercentModifier(speedIncrease)
    addBuff(buff, owner)
end

function applyEffects()
end