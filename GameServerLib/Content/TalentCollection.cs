using System;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Content
{
    public static class TalentContentCollection
    {
        public class TalentCollectionEntry : ContentFile
        {
            public byte MaxLevel => Convert.ToByte(Values["SpellData"]["Ranks"]);
            public object Id => MetaData["Id"];
        }

        private static Dictionary<string, TalentCollectionEntry> _talents = new Dictionary<string, TalentCollectionEntry>();
        private static ContentManager _contentManager;

        public static void Init(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public static bool TalentIsValid(string talentName)
        {
            //Checks if it was already loaded
            if (_talents.ContainsKey(talentName))
            {
                return true;
            }
            //Tries to load
            else
            {
                TalentCollectionEntry talent;
                talent = _contentManager.GetTalentEntry(talentName);
                //If got loaded, it is valid
                if (talent != null)
                {
                    _talents.Add(talentName, talent);
                    return true;
                }
                //Not valid
                else
                {
                    return false;
                }
            }
        }

        public static byte GetTalentMaxRank(string mastery)
        {
            return _talents[mastery].MaxLevel;
        }
    }
}
