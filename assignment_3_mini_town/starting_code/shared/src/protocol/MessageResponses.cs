using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{

    public class MessageResponses:ISerializable
    {
        public List<MessageToSend> messages;

        public void Serialize(Packet pPacket)
        {
            int count = (messages == null ? 0 : messages.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(messages[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            messages = new List<MessageToSend>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                messages.Add(pPacket.Read<MessageToSend>());
            }
        }
    }
}
