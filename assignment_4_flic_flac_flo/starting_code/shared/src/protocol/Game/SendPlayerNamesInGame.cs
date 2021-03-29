using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class SendPlayerNamesInGame : ASerializable
    {
        public string player1;
        public string player2;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(player1);
            pPacket.Write(player2);
        }

        public override void Deserialize(Packet pPacket)
        {
            player1 = pPacket.ReadString();
            player2 = pPacket.ReadString();
        }
    }
}
