Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = Vector2:Mult(to,1150)
    local trueCoords = Vector2:Add(current, range)

    addParticleTarget("Kassadin_Base_Q_ShieldOn.troy", getOwner())
    addParticleTarget("Kassadin_Base_Q_Shield.troy", getOwner())
    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
   -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget("Kassadin_Base_Q_tar.troy", getTarget())
    destroyProjectile()
end
