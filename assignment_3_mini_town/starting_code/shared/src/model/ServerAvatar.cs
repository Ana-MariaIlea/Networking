using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class ServerAvatar : ISerializable
    {
        public int Id;
        public int skinId;
        public int posX;
        public int posY;
        public int posZ;

        public ServerAvatar() { }

        public ServerAvatar(int pId, int pSkinId, int pPosX,int pPosY,int pPosZ)
        {
            Id = pId;
            skinId = pSkinId;
            posX = pPosX;
            posY = pPosY;
            posZ = pPosZ;
        }

        public void ChangePosition(AvatarPositionRequest newPosition)
        {
            posX = newPosition.x;
            posY = newPosition.y;
            posZ = newPosition.z;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(Id);
            pPacket.Write(skinId);
            pPacket.Write(posX);
            pPacket.Write(posY);
            pPacket.Write(posZ);
        }
        public void Deserialize(Packet pPacket)
        {
            Id = pPacket.ReadInt();
            skinId = pPacket.ReadInt();
            posX = pPacket.ReadInt();
            posY = pPacket.ReadInt();
            posZ = pPacket.ReadInt();
        }
    }
}
