using LeagueSandbox.GameServer.Core.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class BuffManager
    {
        private Game _game;
        private Dictionary<string, Buff> _buffDictionary = new Dictionary<string, Buff>();
        private byte _slot = 1;

        public BuffManager(Game game)
        {
            _game = game;
        }

        public bool AddBuff(string buffName, float dur, Unit onto, Unit from)
        {
            if (!_buffDictionary.ContainsKey(buffName))
            {
                var buff = new Buff(this, buffName, dur, 1, _slot++, onto, from);
                _buffDictionary.Add(buffName, buff);
                onto.AddBuff(buff);
                _game.PacketNotifier.notifyAddBuff(buff);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddBuff(string buffName, float dur, Unit onto)
        {
            return AddBuff(buffName, dur, onto, onto);
        }

        public Buff GetBuff(string buffName)
        {
            if (_buffDictionary.ContainsKey(buffName))
            {
                return _buffDictionary[buffName];
            }
            else
            {
                return null;
            }
        }

        public bool RemoveBuff(Buff buff)
        {
            if (_buffDictionary.ContainsKey(buff.GetName()))
            {
                _game.PacketNotifier.notifyRemoveBuff(buff);
                buff.GetUnit().RemoveBuff(buff);
                _buffDictionary.Remove(buff.GetName());
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveBuff(string buffName)
        {
            if (_buffDictionary.ContainsKey(buffName))
            {
                var buff = _buffDictionary[buffName];
                _game.PacketNotifier.notifyRemoveBuff(buff);
                buff.GetUnit().RemoveBuff(buff);
                _buffDictionary.Remove(buffName);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Game GetGame()
        {
            return _game;
        }
    }
}
