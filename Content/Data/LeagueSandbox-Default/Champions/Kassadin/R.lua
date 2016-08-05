Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = Vector2:new(getSpellToX(), getSpellToY()) - current
    local trueCoords

    if(to:length() > 475) then
        to = to:normalize()
        local range = to * 475
        trueCoords = Vector2:new(current.x, current.y) + range
    else
        trueCoords = Vector2:new(getSpellToX(), getSpellToY())
    end

    --addParticle("Kassadin_Base_R_vanish.troy", getOwnerX(), getOwnerY());
    teleportTo(trueCoords.x, trueCoords.y)
    addParticle("Kassadin_Base_R_appear.troy", getOwnerX(), getOwnerY())
end

function applyEffects()
   dealPhysicalDamage(getEffectValue(0))
   -- TODO this can be fetched from projectile inibin "HitEffectName"
   addParticleTarget("Kassadin_Base_Q_Shield.troy", getOwner())
   destroyProjectile()
end
