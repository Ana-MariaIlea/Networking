using shared;
using System;

namespace server
{
	/**
	 * This room runs a single Game (at a time). 
	 * 
	 * The 'Game' is very simple at the moment:
	 *	- all client moves are broadcasted to all clients
	 *	
	 * The game has no end yet (that is up to you), in other words:
	 * all players that are added to this room, stay in here indefinitely.
	 */
	class GameRoom : Room
	{
		public bool IsGameInPlay { get; private set; }
		private TcpMessageChannel p1;
		private TcpMessageChannel p2;

		//wraps the board to play on...
		private TicTacToeBoard _board = new TicTacToeBoard();

		public GameRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		public void StartGame (TcpMessageChannel pPlayer1, TcpMessageChannel pPlayer2)
		{
			if (IsGameInPlay) throw new Exception("Programmer error duuuude.");

			IsGameInPlay = true;
			addMember(pPlayer1);
			addMember(pPlayer2);
			SendPlayerNamesInGame names = new SendPlayerNamesInGame();
			names.player1 = _server.GetPlayerInfo(pPlayer1).name;
			names.player2 = _server.GetPlayerInfo(pPlayer2).name;
			pPlayer1.SendMessage(names);
			pPlayer2.SendMessage(names);
			p1 = pPlayer1;
			p2 = pPlayer2;
		}

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//notify client he has joined a game room 
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.GAME_ROOM;
			pMember.SendMessage(roomJoinedEvent);
		}

		public override void Update()
		{
			//demo of how we can tell people have left the game...
			int oldMemberCount = memberCount;
			base.Update();
			int newMemberCount = memberCount;

			if (oldMemberCount != newMemberCount)
			{
				Log.LogInfo("People left the game...", this);
			}

			if (IsGameInPlay == true)
			{
				TicTacToeBoardData data = _board.GetBoardData();

				if (data.WhoHasWon() != 0)
				{
					IsGameInPlay = false;
					removeMember(p1);
					removeMember(p2);
					_server.GetLobbyRoom().AddMember(p1);
					_server.GetLobbyRoom().AddMember(p2);
					if (data.WhoHasWon() == 1)
					{
						ChatMessage whoWon = new ChatMessage();
						whoWon.message = _server.GetPlayerInfo(p1).name + " has won";
						p1.SendMessage(whoWon);
						p2.SendMessage(whoWon);

						ChatMessage whoLost = new ChatMessage();
						whoLost.message = _server.GetPlayerInfo(p2).name + " has lost";
						p1.SendMessage(whoLost);
						p2.SendMessage(whoLost);
					}
					else
					{
						ChatMessage whoWon = new ChatMessage();
						whoWon.message = _server.GetPlayerInfo(p2).name + " has won";
						p1.SendMessage(whoWon);
						p2.SendMessage(whoWon);

						ChatMessage whoLost = new ChatMessage();
						whoLost.message = _server.GetPlayerInfo(p1).name + " has lost";
						p1.SendMessage(whoLost);
						p2.SendMessage(whoLost);
					}
				}
			}
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is MakeMoveRequest)
			{
				handleMakeMoveRequest(pMessage as MakeMoveRequest, pSender);
			}
		}

		private void handleMakeMoveRequest(MakeMoveRequest pMessage, TcpMessageChannel pSender)
		{
			//we have two players, so index of sender is 0 or 1, which means playerID becomes 1 or 2
			int playerID = indexOfMember(pSender) + 1;
			//make the requested move (0-8) on the board for the player
			_board.MakeMove(pMessage.move, playerID);

			//and send the result of the boardstate back to all clients
			MakeMoveResult makeMoveResult = new MakeMoveResult();
			makeMoveResult.whoMadeTheMove = playerID;
			makeMoveResult.boardData = _board.GetBoardData();
			sendToAll(makeMoveResult);


		}

	}
}
