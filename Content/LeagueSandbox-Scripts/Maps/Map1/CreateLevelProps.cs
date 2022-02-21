using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;


namespace MapScripts.Map1
{
    public static class CreateLevelProps
    {
        public static void CreateProps()
        {
            //Map props
            AddLevelProp("LevelProp_Yonkey", "Yonkey", new Vector3(12465.0f, 101.0f, 14422.257f), new Vector3(0.0f, 66.0f, 0.0f), new Vector3(-33.3334f, 122.2222f, -133.3333f), Vector3.One);
            AddLevelProp("LevelProp_Yonkey1", "Yonkey", new Vector3(-76.0f, 94.0f, 1769.1589f), new Vector3(0.0f, 30.0f, 0.0f), new Vector3(0.0f, -11.1111f, -22.2222f), Vector3.One);
            AddLevelProp("LevelProp_ShopMale", "ShopMale", new Vector3(13374.17f, 194.9741f, 14245.673f), new Vector3(0.0f, 224f, 0.0f), new Vector3(0.0f, 33.3333f, -44.4445f), Vector3.One);
            AddLevelProp("LevelProp_ShopMale1", "ShopMale", new Vector3(-99.5613f, 191.4039f, 855.6632f), new Vector3(0.0f, 158.0f, 0.0f), Vector3.Zero, Vector3.One);
        }
    }
}
