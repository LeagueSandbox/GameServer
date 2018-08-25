using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    class PlayerStats : BasePacket
    {
        public PlayerStats(IChampion player)
            : base(PacketCmd.PKT_S2C_PLAYER_STATS, player.NetId)
        {
            // get the stats and writes the packet
            byte[] stats = player.ChampStats.GetBytes();
            Write(stats.Length);
            Write(stats);

            //4 bytes - length of the packet
            //then all the stats (check ChampionStats for the order)

            //Some stats:
            //ID
            //NAME
            //SKIN
            //TEAM
            //WIN
            //EXP
            //LEVEL
            //GOLD_SPENT
            //GOLD_EARNED
            //NUM_DEATHS
            //CHAMPIONS_KILLED
            //ASSISTS
            //BARRACKS_KILLED
            //TURRETS_KILLED
            //HQ_KILLED
            //MINIONS_KILLED
            //NEUTRAL_MINIONS_KILLED
            //SUPER_MONSTER_KILLED
            //LARGEST_KILLING_SPREE
            //KILLING_SPREES
            //LARGEST_MULTI_KILL
            //TOTAL_DAMAGE_DEALT
            //PHYSICAL_DAMAGE_DEALT_PLAYER
            //MAGIC_DAMAGE_DEALT_PLAYER
            //TOTAL_DAMAGE_DEALT_TO_CHAMPIONS
            //PHYSICAL_DAMAGE_DEALT_TO_CHAMPIONS
            //MAGIC_DAMAGE_DEALT_TO_CHAMPIONS
            //TOTAL_DAMAGE_TAKEN
            //PHYSICAL_DAMAGE_TAKEN
            //MAGIC_DAMAGE_TAKEN
            //DOUBLE_KILLS
            //TRIPLE_KILLS
            //QUADRA_KILLS
            //PENTA_KILLS
            //UNREAL_KILLS
            //ITEMS_PURCHASED
            //CONSUMABLES_PURCHASED
            //SPELL1_CAST
            //SPELL2_CAST
            //SPELL3_CAST
            //SPELL4_CAST
            //SUMMON_SPELL1_CAST
            //SUMMON_SPELL2_CAST
            //LARGEST_CRITICAL_STRIKE
            //TIME_PLAYED
            //LONGEST_TIME_SPENT_LIVING
            //TOTAL_TIME_SPENT_DEAD
            //TOTAL_HEAL
            //TOTAL_UNITS_HEALED
            //FRIENDLY_TURRET_LOST
            //FRIENDLY_DAMPEN_LOST
            //FRIENDLY_HQ_LOST
            //TOOK_FIRST_BLOOD
            //WAS_AFK
            //WAS_AFK_AFTER_FAILED_SURRENDER
            //TIME_OF_FROM_LAST_DISCONNECT
            //TIME_SPENT_DISCONNECTED
            //NEVER_ENTERED_GAME
            //TEAMMATE_NEVER_ENTERED_GAME
            //ITEM0
            //ITEM1
            //ITEM2
            //ITEM3
            //ITEM4
            //ITEM5
            //ITEM6
            //SIGHT_WARDS_BOUGHT_IN_GAME
            //VISION_WARDS_BOUGHT_IN_GAME
            //NODE_CAPTURE
            //NODE_NEUTRALIZE
            //NODE_KILL_OFFENSE
            //TEAM_OBJECTIVE
            //DEFEND_POINT_NEUTRALIZE
            //NODE_KILL_DEFENSE
            //NODE_TIME_DEFENSE
            //LAST_STAND
            //NODE_CAPTURE_ASSIST
            //NODE_NEUTRALIZE_ASSIST
            //TOTAL_PLAYER_SCORE
            //OFFENSE_PLAYER_SCORE
            //DEFENSE_PLAYER_SCORE
            //COMBAT_PLAYER_SCORE
            //OBJECTIVE_PLAYER_SCORE
            //VICTORY_POINT_TOTAL
            //TOTAL_SCORE_RANK
            //PING
            //TRUE_DAMAGE_DEALT_PLAYER
            //TRUE_DAMAGE_TAKEN
            //TRUE_DAMAGE_DEALT_TO_CHAMPIONS
            //WARD_PLACED
            //WARD_KILLED
            //TOTAL_TIME_CROWD_CONTROL_DEALT
            //NEUTRAL_MINIONS_KILLED
            //NEUTRAL_MINIONS_KILLED_YOUR_JUNGLE
            //NEUTRAL_MINIONS_KILLED_ENEMY_JUNGLE

        }
    }
}
