using IntWarsSharp.Core.Logic.RAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Core.Logic.Items
{
    class ItemManager
    {
        private static ItemManager _instance;
        
        internal void init()
        {
            
        }


        public static ItemManager getInstance()
        {
            if (_instance == null)
                _instance = new ItemManager();
            return _instance;
        }
    }
}
