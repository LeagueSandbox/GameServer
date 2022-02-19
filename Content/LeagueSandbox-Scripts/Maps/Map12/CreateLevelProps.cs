using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map12
{
    public static class CreateLevelProps
    {
        public static void CreateProps(ARAM mapScript)
        {
            // Level Props
            AddLevelProp("LevelProp_HA_AP_HeroTower", "HA_AP_HeroTower", new Vector2(1637.6909f, 6079.676f), -3986.0718f, new Vector3(0.0f, 316f, 0.0f), new Vector3(0.0f, -1000.0f, 0.0f), Vector3.One);
            mapScript.LongChains.Add(0, AddLevelProp("LevelProp_HA_AP_Chains_Long", "HA_AP_Chains_Long", new Vector2(2883.2095f, 5173.606f), 86.12982f, new Vector3(0f, 324f, 0f), new Vector3(-88.8889f, 355.5555f, -100.0f), Vector3.One, type: 2));
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue1", "HA_AP_BridgeLaneStatue", new Vector2(4904.28f, 3607.452f), 92.056755f, new Vector3(0f, 134f, 0f), new Vector3(-122.2222f, 177.7777f, 111.1112f), Vector3.One);
            mapScript.Poros.Add(4, AddLevelProp("LevelProp_HA_AP_Poro4", "HA_AP_Poro", new Vector2(10655.31f, 8387.441f), -375.02237f, new Vector3(0f, 222f, 0f), new Vector3(266.6666f, -55.5556f, -11.1111f), Vector3.One));
            mapScript.LongChains.Add(2, AddLevelProp("LevelProp_HA_AP_Chains_Long2", "HA_AP_Chains_Long", new Vector2(5139.6133f, 2801.751f), 97.240906f, new Vector3(0f, 314f, 0f), new Vector3(155.5557f, 366.6666f, -322.2222f), Vector3.One, type: 2));
            AddLevelProp("LevelProp_HA_AP_Hermit", "HA_AP_Hermit", new Vector2(11218.746f, 12037.217f), -164.43712f, new Vector3(0f, 136f, 0.0f), new Vector3(88.8889f, 44.4445f, 30.0f), Vector3.One);
            mapScript.Chains.Add(6, AddLevelProp("LevelProp_HA_AP_Chains6", "HA_AP_Chains", new Vector2(6074.8384f, 3868.2524f), 87.96773f, new Vector3(0f, 318f, 0.0f), new Vector3(-77.7778f, 222.2222f, -44.4445f), Vector3.One));
            mapScript.Chains.Add(4, AddLevelProp("LevelProp_HA_AP_Chains4", "HA_AP_Chains", new Vector2(7492.48f, 5250.153f), 76.85663f, new Vector3(0f, 320f, 0.0f), new Vector3(-22.2222f, 211.1111f, 22.2223f), Vector3.One));
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue4", "HA_AP_BridgeLaneStatue", new Vector2(9189.304f, 7740.729f), 79.74847f, new Vector3(0f, 134f, 0f), new Vector3(-133.3333f, 144.4445f, 122.2222f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue2", "HA_AP_BridgeLaneStatue", new Vector2(6321.7275f, 5004.743f), 88.65647f, new Vector3(0f, 134f, 0f), new Vector3(-144.4445f, 155.5557f, 144.4445f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_ShpNorth", "HA_AP_ShpNorth", new Vector2(10811.736f, 11978.403f), -217.8545f, new Vector3(0.0f, 292f, 0.0f), new Vector3(-211.1111f, -144.4445f, -111.1111f), Vector3.One);
            mapScript.Poros.Add(2, AddLevelProp("LevelProp_HA_AP_Poro2", "HA_AP_Poro", new Vector2(5644.625f, 7807.1074f), -732.70325f, new Vector3(0f, 34f, 0f), new Vector3(-88.8889f, 0f, 0f), Vector3.One));
            mapScript.LongChains.Add(3, AddLevelProp("LevelProp_HA_AP_Chains_Long3", "HA_AP_Chains_Long", new Vector2(7724.4653f, 9869.202f), 86.12982f, new Vector3(0f, 314f, 0f), new Vector3(-33.3334f, 355.5555f, 166.6666f), Vector3.One, type: 2));
            AddLevelProp("LevelProp_HA_AP_ShpSouth", "HA_AP_ShpSouth", new Vector2(521.2136f, 1913.0146f), -186.65932f, new Vector3(0f, 316f, 0.0f), new Vector3(66.6667f, 22.2223f, 11.1111f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue5", "HA_AP_BridgeLaneStatue", new Vector2(7918.951f, 9124.248f), 77.485504f, new Vector3(0f, 316f, 0f), new Vector3(111.1112f, 144.4445f, -111.1111f), Vector3.One);
            mapScript.Poros.Add(3, AddLevelProp("LevelProp_HA_AP_Poro3", "HA_AP_Poro", new Vector2(11036.813f, 12432.2109f), -732.7032f, new Vector3(0f, 166f, 0f), new Vector3(44.4445f, 0f, 0f), Vector3.One));
            mapScript.LongChains.Add(1, AddLevelProp("LevelProp_HA_AP_Chains_Long1", "HA_AP_Chains_Long", new Vector2(9939.937f, 7628.735f), 69.55461f, new Vector3(0f, 320f, 0f), new Vector3(111.1112f, 300.0f, -111.1111f), Vector3.One, type: 2));
            mapScript.Poros.Add(6, AddLevelProp("LevelProp_HA_AP_Poro6", "HA_AP_Poro", new Vector2(6753.814f, 5412.7236f), 48.33051f, new Vector3(0f, 130f, 0f), new Vector3(-22.2222f, -11.1111f, 0f), Vector3.One));
            AddLevelProp("LevelProp_HA_AP_Cutaway", "HA_AP_Cutaway", new Vector2(7815.47f, 7517.7188f), -222.17885f, new Vector3(0f, 314f, 0f), new Vector3(-722.2222f, 177.7777f, 244.4445f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue8", "HA_AP_BridgeLaneStatue", new Vector2(3633.1042f, 4999.506f), 87.18081f, new Vector3(0f, 316f, 0f), new Vector3(144.4445f, 155.5557f, -144.4445f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue7", "HA_AP_BridgeLaneStatue", new Vector2(5055.8486f, 6352.3774f), 86.278946f, new Vector3(0f, 318f, 0f), new Vector3(144.4445f, 155.5557f, -166.6667f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_PeriphBridge", "HA_AP_PeriphBridge", new Vector2(-500.19504f, 17371.6f), -8219.082f, new Vector3(0f, 334f, 0f), new Vector3(-611.1111f, 322.2223f, 88.8889f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_Viking", "HA_AP_Viking", new Vector2(515.8099f, 1919.1678f), -97.770424f, new Vector3(0f, 130f, 0.0f), new Vector3(77.7777f, 111.1112f, 22.2223f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue3", "HA_AP_BridgeLaneStatue", new Vector2(7777.0776f, 6377.4634f), 84.44989f, new Vector3(0f, 136f, 0f), new Vector3(-122.2222f, 155.5557f, 133.3334f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue6", "HA_AP_BridgeLaneStatue", new Vector2(6489.379f, 7746.419f), 88.206276f, new Vector3(0f, 316f, 0f), new Vector3(144.4445f, 155.5557f, -155.5555f), Vector3.One);
            mapScript.Chains.Add(1, AddLevelProp("LevelProp_HA_AP_Chains1", "HA_AP_Chains", new Vector2(3931.2275f, 6102.559f), 87.96773f, new Vector3(0f, 314f, 0.0f), new Vector3(-22.2222f, 222.2222f, -66.6667f), Vector3.One));
            mapScript.Poros.Add(0, AddLevelProp("LevelProp_HA_AP_Poro", "HA_AP_Poro", new Vector2(411.7452f, 515.7069f), 751.8175f, new Vector3(6f, 136f, 8f), new Vector3(-44.4445f, -11.1111f, -77.7778f), Vector3.One));
            AddLevelProp("LevelProp_HA_AP_Hermit_Robot1", "HA_AP_Hermit_Robot", new Vector2(11196.524f, 12129.439f), -208.88162f, new Vector3(0f, 160f, 0.0f), new Vector3(0.0f, 66.6667f, 122.2222f), Vector3.One);
            AddLevelProp("LevelProp_HA_AP_BannerMidBridge", "HA_AP_BannerMidBridge", new Vector2(7088.8677f, 5605.5967f), -757.4999f, new Vector3(0f, 316f, 0.0f), new Vector3(-44.4445f, 111.1112f, 33.3333f), Vector3.One);
            mapScript.Chains.Add(5, AddLevelProp("LevelProp_HA_AP_Chains5", "HA_AP_Chains", new Vector2(8959.606f, 6577.5415f), 87.96773f, new Vector3(0f, 316f, 0.0f), new Vector3(-33.3334f, 222.2222f, -77.7778f), Vector3.One));
            mapScript.Poros.Add(5, AddLevelProp("LevelProp_HA_AP_Poro5", "HA_AP_Poro", new Vector2(12556.904f, 9949.372f), -861.41876f, new Vector3(0f, 182f, 0f), new Vector3(-288.8889f, -22.2222f, 244.4445f), Vector3.One));
            mapScript.Chains.Add(3, AddLevelProp("LevelProp_HA_AP_Chains3", "HA_AP_Chains", new Vector2(6770.9233f, 8888.638f), 65.74553f, new Vector3(0f, 316f, 0.0f), new Vector3(-33.3334f, 200.0f, -33.3334f), Vector3.One));
            mapScript.Chains.Add(2, AddLevelProp("LevelProp_HA_AP_Chains2", "HA_AP_Chains", new Vector2(5348.3027f, 7505.369f), 76.85663f, new Vector3(0f, 316f, 0.0f), new Vector3(11.1111f, 211.1111f, 0f), Vector3.One));
            mapScript.Poros.Add(1, AddLevelProp("LevelProp_HA_AP_Poro1", "HA_AP_Poro", new Vector2(2141.1174f, 4335.2715f), -113.360855f, new Vector3(0f, 208f, 0f), new Vector3(-333.3333f, -55.5556f, 0f), Vector3.One));
        }
    }
}
