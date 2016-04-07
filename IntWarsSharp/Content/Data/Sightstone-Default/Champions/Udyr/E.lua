function finishCasting()
    print("Speed increase: " .. getEffectValue(0))

    local buff = Buff.new("", getEffectValue(1), BUFFTYPE_TEMPORARY, getOwner())

    buff:setMovementSpeedPercentModifier(getEffectValue(0))
    addBuff(buff)
end

function applyEffects()
end
