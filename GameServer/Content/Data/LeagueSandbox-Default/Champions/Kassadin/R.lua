Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = Vector2:new(spell.X, spell.Y) - current
    local trueCoords

    if(to:length() > 475) then
        to = to:normalize()
        local range = to * 475
        trueCoords = Vector2:new(current.x, current.y) + range
    else
        trueCoords = Vector2:new(spell.X, spell.Y)
    end

    --addParticle("Kassadin_Base_R_vanish.troy", owner.X, owner.Y);
    teleportTo(trueCoords.x, trueCoords.y)
    addParticle("Kassadin_Base_R_appear.troy", owner.X, owner.Y)
end

function applyEffects()
   dealPhysicalDamage(getEffectValue(0))
   -- TODO this can be fetched from projectile inibin "HitEffectName"
   addParticleTarget("Kassadin_Base_Q_Shield.troy", owner)
   destroyProjectile()
end
