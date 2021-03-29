using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class SendBoardData : ASerializable
    {
        public TicTacToeBoardData boardData;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(boardData);
        }

        public override void Deserialize(Packet pPacket)
        {
            boardData = pPacket.Read<TicTacToeBoardData>();
        }
    }
}
