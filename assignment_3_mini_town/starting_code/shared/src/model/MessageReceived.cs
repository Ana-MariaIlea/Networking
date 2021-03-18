using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class MessageReceived : ISerializable
    {
        public string text;

        public MessageReceived() { }
        public MessageReceived(string value)
        {
            text = value;
        }
        public void Serialize(Packet pPacket)
        {
            pPacket.Write(text);
        }

        public void Deserialize(Packet pPacket)
        {
            text = pPacket.ReadString();
        }
    }
}
