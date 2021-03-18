using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class SetNewSkinResponse : ISerializable
    {
        public List<AvatarSkin> newSkins;
        public void Serialize(Packet pPacket)
        {
            int count = (newSkins == null ? 0 : newSkins.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(newSkins[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            newSkins = new List<AvatarSkin>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                newSkins.Add(pPacket.Read<AvatarSkin>());
            }
        }
    }
}
