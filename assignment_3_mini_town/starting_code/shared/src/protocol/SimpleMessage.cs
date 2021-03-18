using System.Collections.Generic;


namespace shared
{
    public class SimpleMessage : ISerializable
    {
        public MessageReceived text;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(text);
        }

        public void Deserialize(Packet pPacket)
        {
            text = pPacket.Read<MessageReceived>();
        }


    }
}
