Vector2 = require 'Vector2' -- include 2d vector lib
 
function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = Vector2:new(getSpellToX(), getSpellToY()) - current
    local trueCoords
 
    if to:length() > 475 then
        to = to:normalize()
        local range = to * 475
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(getSpellToX(), getSpellToY())
    end
 
    addParticle(getOwner(), "Ezreal_arcaneshift_cas.troy", getOwnerX(), getOwnerY());
    teleportTo(getOwner(), trueCoords.x, trueCoords.y)
    addParticleTarget(getOwner(), "Ezreal_arcaneshift_flash.troy", getOwner());
 
    local target = nil
    local units = getUnitsInRange(getOwner(), 700, true)
 
    for i=0,units.Count-1 do
		value = units[i]
        local distance = 700
        if getOwner():getTeam() ~= value:getTeam() then
            if Vector2:new(trueCoords.x, trueCoords.y):distance(Vector2:new(value:getX(), value:getY())) <= distance then
                target = value
                distance = Vector2:new(trueCoords.x, trueCoords.y):distance(Vector2:new(value:getX(), value:getY()))
            end
        end
    end
    if target then
        addProjectileTarget("EzrealArcaneShiftMissile", target)
    end
end
 
function applyEffects()
    dealMagicalDamage(25+getSpellLevel()*50+getOwner():GetStats().AbilityPower.Total*0.75)
    addParticleTarget(getOwner(), "Ezreal_arcaneshift_tar.troy", getTarget())
    destroyProjectile()
end
