using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class MoveResponse : ISerializable
    {
        public List<AvatarPosition> positions;

        public void Serialize(Packet pPacket)
        {
            int count = (positions == null ? 0 : positions.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(positions[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            positions = new List<AvatarPosition>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                positions.Add(pPacket.Read<AvatarPosition>());
            }
        }
    }
}
