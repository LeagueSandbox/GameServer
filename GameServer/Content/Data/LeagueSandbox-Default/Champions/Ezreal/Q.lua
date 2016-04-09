Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1150
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealPhysicalDamage(getEffectValue(0)+getOwner():getStats():getTotalAd()+(0.4*getOwner():getStats():getTotalAp()))
    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget("Ezreal_mysticshot_tar.troy", getTarget())

    destroyProjectile()
end
