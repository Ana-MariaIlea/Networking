﻿using System.Collections.Generic;


namespace shared
{
    public class SimpleMessage : ISerializable
    {
           string text;

        //    public void Serialize(Packet pPacket)
        //    {
        //        pPacket.Write(text);
        //    }

        //    public void Deserialize(Packet pPacket)
        //    {
        //        text = pPacket.ReadString();
        //

        public List<ServerAvatar> avatars;
        public void Serialize(Packet pPacket)
        {
            int count = (avatars == null ? 0 : avatars.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(avatars[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            avatars = new List<ServerAvatar>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                avatars.Add(pPacket.Read<ServerAvatar>());
            }
        }
    
    }
}
