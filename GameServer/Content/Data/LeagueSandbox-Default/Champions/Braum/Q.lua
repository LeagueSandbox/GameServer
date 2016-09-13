Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    -- Create projectile for slowing enemies
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range
    
    addProjectile(trueCoords.x, trueCoords.y)
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
    addParticleTarget("Braum_Base_Q_mis.troybin", getTarget())
    
    destroyProjectile()
end
