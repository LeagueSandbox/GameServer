function finishCasting()
	local speed = getEffectValue( 3 )
	local owner = owner
	local myTeam = owner.Team
	local champions = getChampionsInRange( owner, 1300, true )

    print("Speed increase" .. speed)

	for key,value in pairs( champions ) do
		if myTeam == value.Team then
		    local buff = Buff.new( "RaiseMorale", 7.0, BUFFTYPE_TEMPORARY, value )
		    buff:setMovementSpeedPercentModifier( getEffectValue(3) )
		    addBuff( buff, value )
	        addParticleTarget( "pirate_raiseMorale_tar.troy", value )
		end
	end
end

function applyEffects()
end
