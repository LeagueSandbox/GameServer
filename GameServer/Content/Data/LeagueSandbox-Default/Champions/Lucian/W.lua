Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
	local current = Vector2:new(getOwnerX(), getOwnerY())
	local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
	local range = to * 900
	local trueCoords = current + range
	printChat("You used W");
	addProjectile("LucianWMissile", trueCoords.x, trueCoords.y)
end
function applyEffects()
	local damage = 20+getSpellLevel()*40+getOwner():GetStats().AbilityPower.Total*0.9
    dealMagicalDamage(damage)
end