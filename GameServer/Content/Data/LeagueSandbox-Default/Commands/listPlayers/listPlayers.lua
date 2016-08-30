registerCommand("listplayers", "listplayers")

function onExecute()
    players = getAllPlayers()
    printChat("List of players :")
    for i=0,players.Length-1 do
		value = players[i]
        local playerName = value:GetName()
        local champName = value:GetChampion():GetType()
        printChat(i .. " - " .. playerName .." (" .. champName .. ")")
        print(champName)
    end
end