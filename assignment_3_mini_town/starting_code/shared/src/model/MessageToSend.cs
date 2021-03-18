using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class MessageToSend:ISerializable
    {
        public int sender;
        public string text;

        public MessageToSend() { }
        public MessageToSend(string value,int senderId)
        {
            text = value;
            sender = senderId;
        }
        public void Serialize(Packet pPacket)
        {
            pPacket.Write(text);
            pPacket.Write(sender);
        }

        public void Deserialize(Packet pPacket)
        {
            text = pPacket.ReadString();
            sender = pPacket.ReadInt();
        }

        public bool IsMessageCommand()
        {
            return false;
        }
    }
}
