players = {
	["player1"] = {
		["rank"] = "DIAMOND",
		["name"] = "Test",
		["champion"] = "Ezreal",
		["team"] = "BLUE", -- BLUE or PURPLE
		["skin"] = 0,
		["summoner1"] = "HEAL",
		["summoner2"] = "FLASH",
		["ribbon"] = 2, -- [ 1 = Leader (Yellow) ]	[ 2 = Mentor (Blue) ]	[ 4 = Cooperative (Green) ]
		["icon"] = 0 -- Summoner Icon ID
	},
   
	--[[-- uncomment this for more players! you can also add more, up to 12!
	["player2"] = {
		["rank"] = "DIAMOND",
		["name"] = "Test2",
		["champion"] = "Ezreal",
		["team"] = "PURPLE",
		["skin"] = 1,
		["summoner1"] = "FLASH",
		["summoner2"] = "IGNITE",
		["ribbon"] = 2,
      ["icon"] = 0
	},
   --]]

   --[[
	["player3"] = {
		["rank"] = "DIAMOND",
		["name"] = "Test3",
		["champion"] = "Caitlyn",
		["team"] = "BLUE",
		["skin"] = 3,
		["summoner1"] = "CLEANSE",
		["summoner2"] = "TELEPORT",
		["ribbon"] = 2,
      ["icon"] = 0
	}--]]
}
game = {
--[[
1 	- 	Summoner's Rift
6 	- 	Crystal Scar
8 	- 	Twisted Treeline
12 	- 	Howling Abyss
11 	- 	New Summoner's Rift (only on the PBE client)
--]]
	["map"] = 1
}