Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local castTarget = castTarget
    local current = Vector2:new(owner.X, owner.Y)
    if current:distance(Vector2:new(castTarget.X, castTarget.Y)) <= 625 then    
        addProjectileTargetCustom("pirate_parley_tar.troy", 0, castTarget)
    else
        print("Target is too far away")
    end
    
end

function applyEffects()
    local castTarget = castTarget

    if ((not (castTarget == 0)) and (not isDead(castTarget))) then
        local owner = owner
        local damage = getEffectValue(0) + owner:getStats():getTotalAd()
        local newGold = owner:getStats():getGold() + 3 + 1*spellLevel
        
        if castTarget:getStats():getCurrentHealth() >= damage then
            owner:dealDamageTo(castTarget, damage, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL)
        else
            owner:getStats():setGold(newGold)
            owner:dealDamageTo(CastTarget, damage, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL)
        end    
    end

    destroyProjectile()
end
