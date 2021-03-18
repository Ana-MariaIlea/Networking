using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class AvatarPositionRequest:ISerializable
    {
        public int x;
        public int y;
        public int z;

        public AvatarPositionRequest() { }

        public AvatarPositionRequest(int px, int py, int pz)
        {
            x = px;
            y = py;
            z = pz;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(x);
            pPacket.Write(y);
            pPacket.Write(z);
        }

        public void Deserialize(Packet pPacket)
        {
            x = pPacket.ReadInt();
            y = pPacket.ReadInt();
            z = pPacket.ReadInt();
        }
    }
}
