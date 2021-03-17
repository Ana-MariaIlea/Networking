using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class AvatarHandler : ISerializable
    {
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
