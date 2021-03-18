using System.Collections.Generic;


namespace shared
{
    public class SimpleMessage : ISerializable
    {
        public string text;

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
