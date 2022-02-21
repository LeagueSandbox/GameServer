using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map10
{
    public static class CreateLevelProps
    {
        public static void CreateProps()
        {
            //Map props
            AddLevelProp("LevelProp_TT_Shopkeeper1", "TT_Shopkeeper", new Vector3(14169.09f, 178.19215f, 7916.989f), new Vector3(0.0f, 150f, 0.0f), new Vector3(22.2223f, 33.3333f, -66.6667f), Vector3.One);
            AddLevelProp("LevelProp_TT_Shopkeeper", "TT_Shopkeeper", new Vector3(1241.6655f, 184.21965f, 7916.2354f), new Vector3(0.0f, 208.0f, 0.0f), new Vector3(-66.6667f, 22.2223f, -55.5556f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane", new Vector3(3624.281f, -100.43866f, 3730.965f), Vector3.Zero, new Vector3(88.8889f, -33.3334f, 66.6667f), Vector3.One);
            AddLevelProp("LevelProp_TT_Nexus_Gears", "TT_Nexus_Gears", new Vector3(3000.0f, 19.51249f, 7289.6816f), Vector3.Zero, new Vector3(0.0f, 144.4445f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier1", "TT_Brazier", new Vector3(1372.0352f, 580.103f, 5049.9087f), new Vector3(0.0f, 134.0f, 0.0f), new Vector3(11.1111f, 288.8889f, -22.2222f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier2", "TT_Brazier", new Vector3(390.23776f, 663.7761f, 6517.922f), Vector3.Zero, new Vector3(-33.3334f, 277.7778f, -11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier3", "TT_Brazier", new Vector3(399.4241f, 692.22107f, 8021.0566f), Vector3.Zero, new Vector3(-22.2222f, 300f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier4", "TT_Brazier", new Vector3(1314.2941f, 582.84155f, 9495.576f), new Vector3(0.0f, 48.0f, 0.0f), new Vector3(-33.3334f, 277.7778f, 22.2223f), Vector3.One);
            AddLevelProp("LevelProp_TT_Speedshrine_Gears", "TT_Speedshrine_Gears", new Vector3(7706.3057f, -124.93201f, 6720.3916f), Vector3.Zero, Vector3.Zero, Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier5", "TT_Brazier", new Vector3(14091.11f, 582.84155f, 9530.338f), new Vector3(0.0f, 120.0f, 0.0f), new Vector3(11.1111f, 277.7778f, 0.0f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier6", "TT_Brazier", new Vector3(14990.463f, 675.81445f, 8053.91f), Vector3.Zero, new Vector3(-22.2222f, 266.6666f, -11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier7", "TT_Brazier", new Vector3(15016.35f, 664.7033f, 6532.84f), Vector3.Zero, new Vector3(-11.1111f, 255.5555f, -11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Brazier8", "TT_Brazier", new Vector3(14102.986f, 580.504f, 5098.367f), new Vector3(0.0f, 36.0f, 0.0f), new Vector3(0.0f, 244.4445f, 11.1111f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Order_Base", "TT_Chains_Order_Base", new Vector3(3778.3638f, -496.0713f, 7573.525f), Vector3.Zero, new Vector3(-233.3334f, -333.3333f, 277.7778f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Xaos_Base", "TT_Chains_Xaos_Base", new Vector3(11636.063f, -551.62683f, 7618.6665f), Vector3.Zero, new Vector3(200.0f, -388.8889f, 333.3334f), Vector3.One);
            AddLevelProp("LevelProp_TT_Chains_Order_Periph", "TT_Chains_Order_Periph", new Vector3(759.1779f, 507.98825f, 4740.9385f), Vector3.Zero, new Vector3(-155.5555f, 44.4445f, 222.2222f), Vector3.One);
            AddLevelProp("LevelProp_TT_Nexus_Gears1", "TT_Nexus_Gears", new Vector3(12392.034f, -2.709816f, 7244.363f), new Vector3(0.0f, 180.0f, 0.0f), new Vector3(-44.4445f, 122.2222f, -122.2222f), Vector3.One);
        }
    }
}
