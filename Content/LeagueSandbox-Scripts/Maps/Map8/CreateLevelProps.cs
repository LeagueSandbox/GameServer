using GameServerCore.Enums;
using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map8
{
    public static class CreateLevelProps
    {
        public static void CreateProps(ODIN mapScript)
        {
            // Level Props
            AddLevelProp("LevelProp_Odin_Windmill_Gears", "Odin_Windmill_Gears", new Vector2(6946.143f, 11918.931f), -122.93308f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(11.1111f, 77.7777f, -122.2222f), Vector3.One);
            AddLevelProp("LevelProp_Odin_Windmill_Propellers", "Odin_Windmill_Propellers", new Vector2(6922.032f, 11940.535f), -259.16052f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-22.2222f, 0.0f, -111.1111f), Vector3.One);
            AddLevelProp("LevelProp_Odin_Lifts_Buckets", "Odin_Lifts_Buckets", new Vector2(2123.782f, 8465.207f), -122.9331f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(188.8889f, 77.7777f, 444.4445f), Vector3.One);
            AddLevelProp("LevelProp_Odin_Lifts_Crystal", "Odin_Lifts_Crystal", new Vector2(1578.0967f, 7505.5938f), -78.48851f, new Vector3(0.0f, 12.0f, 0.0f), new Vector3(-233.3335f, 100.0f, -544.4445f), Vector3.One);
            AddLevelProp("LevelProp_OdinRockSaw02", "OdinRockSaw", new Vector2(5659.9004f, 1016.47925f), -11.821701f, new Vector3(0.0f, 40.0f, 0.0f), new Vector3(233.3334f, 133.3334f, -77.7778f), Vector3.One);
            AddLevelProp("LevelProp_OdinRockSaw01", "OdinRockSaw", new Vector2(2543.822f, 1344.957f), -56.266106f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-122.2222f, 111.1112f, -744.4445f), Vector3.One);
            AddLevelProp("LevelProp_Odin_Drill", "Odin_Drill", new Vector2(11992.028f, 8547.805f), 343.7337f, new Vector3(0.0f, 244.0f, 0.0f), new Vector3(33.3333f, 311.1111f, 0.0f), Vector3.One);
            mapScript.TeamStairs.Add(TeamId.TEAM_BLUE, AddLevelProp("LevelProp_Odin_SoG_Order", "Odin_SoG_Order", new Vector2(266.77225f, 3903.9998f), 139.9266f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-288.8889f, 122.2222f, -188.8889f), Vector3.One));
            AddLevelProp("LevelProp_OdinClaw", "OdinClaw", new Vector2(5187.914f, 2122.2627f), 261.546f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(422.2223f, 255.5555f, -200.0f), Vector3.One);
            mapScript.SwainBeams.Add(1, AddLevelProp("LevelProp_SwainBeam1", "SwainBeam", new Vector2(7207.073f, 1995.804f), 461.54602f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-422.2222f, 355.5555f, -311.1111f), Vector3.One));
            mapScript.SwainBeams.Add(2, AddLevelProp("LevelProp_SwainBeam2", "SwainBeam", new Vector2(8142.406f, 2716.4258f), 639.324f, new Vector3(0.0f, 152.0f, 0.0f), new Vector3(-222.2222f, 444.4445f, -88.8889f), Vector3.One));
            mapScript.SwainBeams.Add(3, AddLevelProp("LevelProp_SwainBeam3", "SwainBeam", new Vector2(9885.076f, 3339.1853f), 350.435f, new Vector3(0.0f, 54.0f, 0.0f), new Vector3(144.4445f, 300.0f, -155.5555f), Vector3.One));
            mapScript.TeamStairs.Add(TeamId.TEAM_PURPLE, AddLevelProp("LevelProp_Odin_SoG_Chaos", "Odin_SoG_Chaos", new Vector2(13623.644f, 3884.6233f), 117.7046f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(288.8889f, 111.1112f, -211.1111f), Vector3.One));
            AddLevelProp("LevelProp_OdinCrane", "OdinCrane", new Vector2(10287.527f, 10776.917f), -145.15509f, new Vector3(0.0f, 52.0f, 0.0f), new Vector3(-22.2222f, 66.6667f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_OdinCrane1", "OdinCrane", new Vector2(9418.097f, -189.59952f), 12105.366f, new Vector3(0.0f, 118.0f, 0.0f), new Vector3(0.0f, 44.4445f, 0.0f), Vector3.One);
            mapScript.Nexus.Add(TeamId.TEAM_BLUE, AddLevelProp("LevelProp_Odin_SOG_Order_Crystal", "Odin_SOG_Order_Crystal", new Vector2(1618.3121f, 4357.871f), 336.9458f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-122.2222f, 277.7778f, -122.2222f), Vector3.One));
            mapScript.Nexus.Add(TeamId.TEAM_PURPLE, AddLevelProp("LevelProp_Odin_SOG_Chaos_Crystal", "Odin_SOG_Chaos_Crystal", new Vector2(12307.629f, 4535.6484f), 225.8346f, new Vector3(0.0f, 214.0f, 0.0f), new Vector3(144.4445f, 222.2222f, -33.3334f), Vector3.One));
        }
    }
}
