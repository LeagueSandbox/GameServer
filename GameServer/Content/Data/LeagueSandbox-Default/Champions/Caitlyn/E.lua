Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    -- Create projectile for slowing enemies
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range
    
    addProjectile("CaitlynEntrapmentMissile", trueCoords.x, trueCoords.y)

    -- Apply knockback
    local original = Vector2:new(owner.X, owner.Y)
    to = Vector2:new(spell.X, spell.Y)
    current = (Vector2:new(owner.X, owner.Y) - to):normalize()
    range = current * 400
    trueCoords = original + range

    dashTo(owner, trueCoords.x, trueCoords.y, 1000, "SPELL3B")
end

function applyEffects()
    -- Damage target
    dealMagicalDamage(getEffectValue(1) + (0.8 * owner:getStats():getTotalAp()))
    addParticleTarget("caitlyn_entrapment_cas.troy", owner)
    addParticleTarget("caitlyn_entrapment_tar.troy", getTarget())
    
    -- Apply slow
    local duration = getEffectValue(2)
    local buff = Buff.new("CaitlynEntrapmentMissile", duration, BUFFTYPE_TEMPORARY, getTarget(), owner)
    
    print("Duration is" .. duration)
    
    buff:setMovementSpeedPercentModifier(-getEffectValue(3)) -- negative value to slow target
    addBuff(buff, getTarget())
    addParticleTarget("caitlyn_entrapment_slow.troy", getTarget())
    
    destroyProjectile()
end
