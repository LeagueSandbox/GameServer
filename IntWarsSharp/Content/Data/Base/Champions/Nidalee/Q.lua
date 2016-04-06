Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1500
    local trueCoords = current + range

    addServerProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealMagicalDamage(getEffectValue(0))

    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget("Nidalee_Base_Q_Tar.troy", getTarget())

    destroyProjectile()
end
