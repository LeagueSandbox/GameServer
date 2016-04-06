function finishCasting()
    print("Speed increase: " ..getEffectValue(2))
    
    local buff = Buff.new("", getEffectValue(3), BUFFTYPE_TEMPORARY, getOwner())
    
    buff:setMovementSpeedPercentModifier(getEffectValue(2)) -- It doesn't work at spell level 1, I don't know why.
    addBuff(buff)
end

function applyEffects()
end
