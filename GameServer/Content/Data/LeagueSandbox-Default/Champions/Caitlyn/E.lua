Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    -- Create projectile for slowing enemies
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range
    
    addProjectile(trueCoords.x, trueCoords.y)

    -- Apply knockback
    local original = Vector2:new(getOwnerX(), getOwnerY())
    to = Vector2:new(getSpellToX(), getSpellToY())
    current = (Vector2:new(getOwnerX(), getOwnerY()) - to):normalize()
    range = current * 400
    trueCoords = original + range

    dashTo(getOwner(), trueCoords.x, trueCoords.y, 1000)
    setAnimation("RUN", "SPELL3B", getOwner())
    
    resetAnimations(getOwner())
end

function applyEffects()
    -- Damage target
    dealMagicalDamage(getEffectValue(1) + (0.8 * getOwner():getStats():getTotalAp()))
    addParticleTarget("caitlyn_entrapment_cas.troy", getOwner())
    addParticleTarget("caitlyn_entrapment_tar.troy", getTarget())
    
    -- Apply slow
    local duration = getEffectValue(2)
    local buff = Buff.new("CaitlynEntrapmentMissile", duration, BUFFTYPE_TEMPORARY, getTarget(), getOwner())
    
    print("Duration is" .. duration)
    
    buff:setMovementSpeedPercentModifier(-getEffectValue(3)) -- negative value to slow target
    addBuff(buff, getTarget())
    addParticleTarget("caitlyn_entrapment_slow.troy", getTarget())
    
    destroyProjectile()
end
