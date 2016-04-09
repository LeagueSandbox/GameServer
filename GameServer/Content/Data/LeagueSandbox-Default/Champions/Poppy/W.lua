function finishCasting()
    print("Speed increase: " ..getEffectValue(2))

    local buff = Buff.new("", 5.0, getOwner())

    buff:setMovementSpeedPercentModifier(getEffectValue(2))
    addBuff(buff)
end

function applyEffects()
end
