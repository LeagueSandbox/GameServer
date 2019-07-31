namespace GameServerCore.Domain
{
    public interface IContentFile
    {
        string GetObject(string section, string name);
        string GetString(string section, string name, string defaultValue = "");
        float GetFloat(string section, string name, float defaultValue = 0);
        int GetInt(string section, string name, int defaultValue = 0);
        bool GetBool(string section, string name, bool defaultValue = false);
        float[] GetFloatArray(string section, string name, float[] defaultValue);
        int[] GetIntArray(string section, string name, int[] defaultValue);
        float[] GetMultiFloat(string section, string name, int num = 6, float defaultValue = 0);
        int[] GetMultiInt(string section, string name, int num = 6, int defaultValue = 0);
    }
}
