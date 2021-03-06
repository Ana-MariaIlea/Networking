﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class AvatarPosition:ISerializable
    {
        public int x;
        public int y;
        public int z;
        public int senderId;

        public AvatarPosition() { }

        public AvatarPosition(int px, int py, int pz, int pSenderId)
        {
            x = px;
            y = py;
            z = pz;
            senderId = pSenderId;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(x);
            pPacket.Write(y);
            pPacket.Write(z);
            pPacket.Write(senderId);
        }

        public void Deserialize(Packet pPacket)
        {
            x = pPacket.ReadInt();
            y = pPacket.ReadInt();
            z = pPacket.ReadInt();
            senderId = pPacket.ReadInt();
        }
    }
}
