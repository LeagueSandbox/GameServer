function finishCasting()
    print("Speed increase: " .. getEffectValue(2))

    local buff = Buff.new("", 2.5, BUFFTYPE_TEMPORARY, getOwner())

    buff:setMovementSpeedPercentModifier(getEffectValue(2))
    addBuff(buff)
end

function applyEffects()
end
