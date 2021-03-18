using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class GenericClientBool : ISerializable
    {
        public int id;
        public bool cond;

        public GenericClientBool() { }

        public GenericClientBool(int pSenderId,bool condition)
        {
            id = pSenderId;
            cond = condition;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(id);
            pPacket.Write(cond);
        }

        public void Deserialize(Packet pPacket)
        {
            id = pPacket.ReadInt();
            cond = pPacket.ReadBool();
        }
    }
}
