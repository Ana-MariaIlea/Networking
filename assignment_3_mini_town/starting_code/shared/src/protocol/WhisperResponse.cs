using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class WhisperResponse : ISerializable
    {
        public MessageToSend messege;
        public List<GenericClientBool> clients;

        public void Serialize(Packet pPacket)
        {

            pPacket.Write(messege);


            int count = (clients == null ? 0 : clients.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(clients[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            messege = new MessageToSend();

            messege = pPacket.Read<MessageToSend>();


            clients = new List<GenericClientBool>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                clients.Add(pPacket.Read<GenericClientBool>());
            }
        }
    }
}

