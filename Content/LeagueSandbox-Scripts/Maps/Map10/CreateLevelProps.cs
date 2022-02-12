using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map10
{
    public static class CreateLevelProps
    {
        public static void CreateProps()
        {
            //Map props
            AddLevelProp("LevelProp_TT_Shopkeeper1", "TT_Shopkeeper", new Vector2(14169.09f, 7916.989f), 178.19215f, new Vector3(0.0f, 150f, 0.0f), new Vector3(22.2223f, 33.3333f, -66.6667f), Vector3.One);
            AddLevelProp("LevelProp_TT_Shopkeeper", "TT_Shopkeeper", new Vector2(1241.6655f, 7916.2354f), 184.21965f, new Vector3(0.0f, 208.0f, 0.0f), new Vector3(-66.6667f, 22.2223f, -55.5556f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane", new Vector2(3624.281f, 3730.965f), -100.43866f, Vector3.Zero, new Vector3(88.8889f, -33.3334f, 66.6667f), Vector3.One);
            AddLevelProp("LevelProp_TT_Nexus_Gears", "TT_Nexus_Gears", new Vector2(3000.0f, 7289.6816f), 19.51249f, Vector3.Zero, new Vector3(0.0f, 144.4445f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier1", "TT_Brazier", new Vector2(1372.0352f, 5049.9087f), 580.103f, new Vector3(0.0f, 134.0f, 0.0f), new Vector3(11.1111f, 288.8889f, -22.2222f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier2", "TT_Brazier", new Vector2(390.23776f, 6517.922f), 663.7761f, Vector3.Zero, new Vector3(-33.3334f, 277.7778f, -11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier3", "TT_Brazier", new Vector2(399.4241f, 8021.0566f), 692.22107f, Vector3.Zero, new Vector3(-22.2222f, 300f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier4", "TT_Brazier", new Vector2(1314.2941f, 9495.576f), 582.84155f, new Vector3(0.0f, 48.0f, 0.0f), new Vector3(-33.3334f, 277.7778f, 22.2223f), Vector3.One);
            AddLevelProp("LevelProp_TT_Speedshrine_Gears", "TT_Speedshrine_Gears", new Vector2(7706.3057f, 6720.3916f), -124.93201f, Vector3.Zero, Vector3.Zero, Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier5", "TT_Brazier", new Vector2(14091.11f, 9530.338f), 582.84155f, new Vector3(0.0f, 120.0f, 0.0f), new Vector3(11.1111f, 277.7778f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier6", "TT_Brazier", new Vector2(14990.463f, 8053.91f), 675.81445f, Vector3.Zero, new Vector3(-22.2222f, 266.6666f, -11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier7", "TT_Brazier", new Vector2(15016.35f, 6532.84f), 664.7033f, Vector3.Zero, new Vector3(-11.1111f, 255.5555f, -11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier8", "TT_Brazier", new Vector2(14102.986f, 5098.367f), 580.504f, new Vector3(0.0f, 36.0f, 0.0f), new Vector3(0.0f, 244.4445f, 11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Order_Base", "TT_Chains_Order_Base", new Vector2(3778.3638f, 7573.525f), -496.0713f, Vector3.Zero, new Vector3(-233.3334f, -333.3333f, 277.7778f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Xaos_Base", "TT_Chains_Xaos_Base", new Vector2(11636.063f, 7618.6665f), -551.62683f, Vector3.Zero, new Vector3(200.0f, -388.8889f, 333.3334f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Order_Periph", "TT_Chains_Order_Periph", new Vector2(759.1779f, 4740.9385f), 507.98825f, Vector3.Zero, new Vector3(-155.5555f, 44.4445f, 222.2222f), Vector3.One);
            AddLevelProp("LevelProp_TT_Nexus_Gears1", "TT_Nexus_Gears", new Vector2(12392.034f, 7244.363f), -2.709816f, new Vector3(0.0f, 180.0f, 0.0f), new Vector3(-44.4445f, 122.2222f, -122.2222f), Vector3.One);
        }
    }
}
