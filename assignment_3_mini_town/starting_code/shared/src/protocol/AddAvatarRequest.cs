using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class AddAvatarRequest : ISerializable
    {
        public ServerAvatar avatar;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(avatar);
        }

        public void Deserialize(Packet pPacket)
        {
            avatar = pPacket.Read<ServerAvatar>();
        }
    }


}

