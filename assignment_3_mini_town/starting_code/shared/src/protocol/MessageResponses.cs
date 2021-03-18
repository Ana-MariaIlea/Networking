using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{

    public class MessageResponses:ISerializable
    {
        public List<Message> messeges;

        public void Serialize(Packet pPacket)
        {
            int count = (messeges == null ? 0 : messeges.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(messeges[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            messeges = new List<Message>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                messeges.Add(pPacket.Read<Message>());
            }
        }
    }
}
