using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class AvatarSkin:ISerializable
    {
        public int skinId;
        public int senderId;

        public AvatarSkin() { }

        public AvatarSkin(int pSkinId, int pSenderId)
        {
            skinId = pSkinId;
            senderId = pSenderId;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(skinId);
            pPacket.Write(senderId);
        }

        public void Deserialize(Packet pPacket)
        {
            skinId = pPacket.ReadInt();
            senderId = pPacket.ReadInt();
        }
    }
}
