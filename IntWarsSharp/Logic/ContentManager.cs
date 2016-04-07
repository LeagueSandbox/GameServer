using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    public class ContentManager
    {
        public ContentManager() { }

        private string GetContentPath(ContentType type, string filename)
        {
            var package = "Base";
            return string.Format("Content/Data/{0}/{1}/{2}", package, type.ContentRoot, filename);
        }

        public string GetMapDataPath(int mapId)
        {
            return GetContentPath(ContentType.MAP, string.Format("map{0}.json", mapId));
        }

        public string GetSpellScriptPath(string champion, string spell)
        {
            return GetContentPath(ContentType.CHAMPION, string.Format("{0}/{1}.lua", champion, spell));
        }
    }

    public class ContentType
    {
        public static readonly ContentType MAP = new ContentType("Maps");
        public static readonly ContentType CHAMPION = new ContentType("Champions");

        public string ContentRoot { get; private set; }
        private ContentType(string root)
        {
            ContentRoot = root;
        }
    }
}
