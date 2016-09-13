function finishCasting()
    local duration = 0.75 + spellLevel * 0.75
    -- print("Speed increase 35")
    -- local buff = Buff.new("", duration, BUFFTYPE_TEMPORARY,  owner)
    -- buff:setMovementSpeedPercentModifier(35)
    -- addBuff(buff) -- The server does not load Garen's stats
    addMovementSpeedBuff(owner, 35, duration) -- doesn't work, but it's closer
end

function applyEffects()
end