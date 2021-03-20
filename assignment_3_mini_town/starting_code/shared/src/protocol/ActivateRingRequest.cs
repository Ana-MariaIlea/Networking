using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class ActivateRingRequest : ISerializable
    {
        public int Id;
        public void Deserialize(Packet pPacket)
        {
            Id = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(Id);
        }
    }
}
