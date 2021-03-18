using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class MoveRequest:ISerializable
    {
        public AvatarPositionRequest position;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(position);
        }

        public void Deserialize(Packet pPacket)
        {
            position = pPacket.Read<AvatarPositionRequest>();
        }
    }
}
