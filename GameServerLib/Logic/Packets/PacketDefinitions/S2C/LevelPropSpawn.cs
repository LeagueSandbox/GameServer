using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LevelPropSpawn : BasePacket
    {
        public LevelPropSpawn(LevelProp lp)
            : base(PacketCmd.PKT_S2C_LEVEL_PROP_SPAWN)
        {
            _buffer.Write((int)lp.NetId);
            _buffer.Write((byte)0x40); // unk
            _buffer.Write(lp.SkinId);
            _buffer.Write((byte)0);
            _buffer.Write((byte)0);
            _buffer.Write((byte)0); // Unk
            _buffer.Write(lp.X);
            _buffer.Write(lp.Z);
            _buffer.Write(lp.Y);
            _buffer.Write(0.0f); // Rotation Y

            _buffer.Write(lp.DirX);
            _buffer.Write(lp.DirZ);
            _buffer.Write(lp.DirY);
            _buffer.Write(lp.Unk1);
            _buffer.Write(lp.Unk2);

            _buffer.Write(1.0f);
            _buffer.Write(1.0f);
            _buffer.Write(1.0f); // Scaling
            _buffer.Write((int)lp.Team); // Probably a short
            _buffer.Write(2); // nPropType [size 1 . 4] (4.18) -- if is a prop, become unselectable and use direction params

            foreach (var b in Encoding.Default.GetBytes(lp.Name))
                _buffer.Write(b);
            _buffer.Fill(0, 64 - lp.Name.Length);
            foreach (var b in Encoding.Default.GetBytes(lp.Model))
                _buffer.Write(b);
            _buffer.Fill(0, 64 - lp.Model.Length);
        }
    }
}