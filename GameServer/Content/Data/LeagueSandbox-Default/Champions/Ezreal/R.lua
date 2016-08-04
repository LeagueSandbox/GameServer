Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 20000
    local trueCoords = current + range

    addProjectile("EzrealTrueshotBarrage", trueCoords.x, trueCoords.y)	
end

function applyEffects()
    --Uncomment these when getNumberObjectsHit() is fixed
    --local reduc = math.min(getNumberObjectsHit(), 3)
    local bonusAD = getOwner():GetStats().AttackDamage.Total - getOwner():GetStats().AttackDamage.BaseValue
    local AP = getOwner():GetStats().AbilityPower.Total*0.9
    local damage = 200+getSpellLevel()*150 + bonusAD + AP
	dealMagicalDamage(damage) -- damage * (1 - reduc/10)
    
    addParticleTarget(getOwner(), "Ezreal_TrueShot_tar.troy", getTarget())
end
