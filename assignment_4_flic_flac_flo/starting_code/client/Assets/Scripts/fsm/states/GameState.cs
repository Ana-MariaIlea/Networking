using shared;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView>
{
    //just for fun we keep track of how many times a player clicked the board
    //note that in the current application you have no idea whether you are player 1 or 2
    //normally it would be better to maintain this sort of info on the server if it is actually important information
    private int player1MoveCount = 0;
    private int player2MoveCount = 0;

    public override void EnterState()
    {
        base.EnterState();

        view.gameBoard.OnCellClicked += _onCellClicked;
    }

    private void _onCellClicked(int pCellIndex)
    {
        MakeMoveRequest makeMoveRequest = new MakeMoveRequest();
        makeMoveRequest.move = pCellIndex;

        fsm.channel.SendMessage(makeMoveRequest);
    }

    public override void ExitState()
    {
        base.ExitState();
        view.gameBoard.OnCellClicked -= _onCellClicked;
    }

    private void Update()
    {
        receiveAndProcessNetworkMessages();
    }

    protected override void handleNetworkMessage(ASerializable pMessage)
    {
        if (pMessage is SendPlayerNamesInGame)
        {
            handlePlayerNames(pMessage as SendPlayerNamesInGame);
        }
        else if (pMessage is SendBoardData)
        {
            HandleBoardDataChange(pMessage as SendBoardData);
        }
        else
        if (pMessage is MakeMoveResult)
        {
            handleMakeMoveResult(pMessage as MakeMoveResult);
        }
        else if (pMessage is RoomJoinedEvent) handleRoomJoinedEvent(pMessage as RoomJoinedEvent);
    }

    private void handleRoomJoinedEvent(RoomJoinedEvent pMessage)
    {
        //did we move to the game room?
        if (pMessage.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        }
    }
    void handlePlayerNames(SendPlayerNamesInGame names)
    {
        view.player1Name = names.player1;
        view.player2Name = names.player2;
        view.playerLabel1.text = view.player1Name;
        view.playerLabel2.text = view.player2Name;
    }

    private void HandleBoardDataChange(SendBoardData sendBoardData)
    {
        view.gameBoard.SetBoardData(sendBoardData.boardData);
    }

    private void handleMakeMoveResult(MakeMoveResult pMakeMoveResult)
    {
        view.gameBoard.SetBoardData(pMakeMoveResult.boardData);

        //some label display
        if (pMakeMoveResult.whoMadeTheMove == 1)
        {
            player1MoveCount++;
            view.playerLabel1.text = view.player1Name + " Movecount: " + player1MoveCount;
        }
        if (pMakeMoveResult.whoMadeTheMove == 2)
        {
            player2MoveCount++;
            view.playerLabel2.text = view.player2Name + " Movecount: " + player2MoveCount;
        }

    }
}
