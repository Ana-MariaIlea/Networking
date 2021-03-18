using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class MessageToSend : ISerializable
    {
        public int sender;
        public string text;

        public MessageToSend() { }
        public MessageToSend(string value, int senderId)
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
            
            if (text.Split(' ')[0] == "/whisper"|| text.Split(' ')[0] == "/setskin") return true;
            return false;
        }

        public string GetCommand()
        {
            return text.Split(' ')[0];
        }

        public string GetRestOfMessage()
        {
            string[] subStrings=text.Split(' ');
            string newString=null;
            for (int i = 1; i < subStrings.Length; i++)
            {
                newString += subStrings[i];
            }

            return newString;
        }
    }
}
