using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/**
 * The main ChatLobbyClient where you will have to do most of your work.
 * 
 * @author J.C. Wichman
 */
public class ChatLobbyClient : MonoBehaviour
{
    //reference to the helper class that hides all the avatar management behind a blackbox
    private AvatarAreaManager _avatarAreaManager;
    //reference to the helper class that wraps the chat interface
    private PanelWrapper _panelWrapper;

    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    private TcpClient _client;

    private void Start()
    {
        connectToServer();

        //register for the important events
        _avatarAreaManager = FindObjectOfType<AvatarAreaManager>();
        _avatarAreaManager.OnAvatarAreaClicked += onAvatarAreaClicked;

        _panelWrapper = FindObjectOfType<PanelWrapper>();
        _panelWrapper.OnChatTextEntered += onChatTextEntered;
    }

    private void connectToServer()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(_server, _port);
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to server:");
            Debug.Log(e.Message);
        }
    }

    private void onAvatarAreaClicked(Vector3 pClickPosition)
    {
        Debug.Log("ChatLobbyClient: you clicked on " + pClickPosition);
        MoveRequest move = new MoveRequest();
        move.position = new AvatarPositionRequest((int)pClickPosition.x, (int)pClickPosition.y, (int)pClickPosition.z);
        sendObject(move);
        //TODO pass data to the server so that the server can send a position update to all clients (if the position is valid!!)
    }

    private void onChatTextEntered(string pText)
    {
        _panelWrapper.ClearInput();
        sendString(pText);
    }

    private void sendString(string pOutString)
    {
        try
        {
            Debug.Log("Sending:" + pOutString);
            SimpleMessage message = new SimpleMessage();
            message.text = new MessageReceived(pOutString);
            sendObject(message);
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }
    private void sendObject(ISerializable pOutObject)
    {
        try
        {
            Debug.Log("Sending:" + pOutObject);

            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);

            StreamUtil.Write(_client.GetStream(), outPacket.GetBytes());
        }

        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }

    // RECEIVING CODE

    private void Update()
    {
        try
        {
            if (_client.Available > 0)
            {
                Debug.Log("Something was sent");
                byte[] inBytes = StreamUtil.Read(_client.GetStream());
                Packet inPacket = new Packet(inBytes);
                ISerializable inObject = inPacket.ReadObject();
                
                if (inObject is AvatarHandler) { handleNewAvatar(inObject as AvatarHandler); }
                if (inObject is MessageResponses) { showMessages(inObject as MessageResponses); }
                if (inObject is MoveResponse) { handleMoveResponse(inObject as MoveResponse); }
            }
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }

    private void handleNewAvatar(AvatarHandler plist)
    {
        Debug.Log("Package received");
        List<int> presentAvatarIds = _avatarAreaManager.GetAllAvatarIds();
        List<int> presentAvatarIdsToRemove = new List<int>();
        foreach (var item in presentAvatarIds)
        {
            if (!plist.HasId(item))
            {
                presentAvatarIdsToRemove.Add(item);
            }
        }

        foreach (var item in presentAvatarIdsToRemove)
        {
            _avatarAreaManager.RemoveAvatarView(item);
        }

        List<ServerAvatar> avatarHandlers = plist.avatars;

        foreach (var avatar in avatarHandlers)
        {
            if (!_avatarAreaManager.HasAvatarView(avatar.Id))
            {

                AvatarView avatarView = _avatarAreaManager.AddAvatarView(avatar.Id);
                avatarView.transform.localPosition = new Vector3(avatar.posX, avatar.posY, avatar.posZ);
                avatarView.SetSkin(avatar.skinId);
                Debug.Log("Avatar Added at "+ avatarView.transform.localPosition);

            }
        }

  
    }

    private void handleMoveResponse(MoveResponse pResponse)
    {
        Debug.Log("new positions received");
        List<AvatarPosition> m = pResponse.positions;
        foreach (var item in m)
        {
            Debug.Log(item.senderId);
            AvatarView avatarView = _avatarAreaManager.GetAvatarView(item.senderId);
            avatarView.Move(new Vector3(item.x, item.y, item.z));
        }
    }
    private void showMessages(MessageResponses pMessages)
    {
        Debug.Log("messeges received");
        List<MessageToSend> m = pMessages.messeges;
        foreach (var item in m)
        {
            Debug.Log(item.text + "  " + item.sender);
            AvatarView avatarView = _avatarAreaManager.GetAvatarView(item.sender);
            avatarView.Say(item.text);
        }
    }
    private void showMessage(string pText)
    {
        //This is a stub for what should actually happen
        //What should actually happen is use an ID that you got from the server, to get the correct avatar
        //and show the text message through that
        List<int> allAvatarIds = _avatarAreaManager.GetAllAvatarIds();
        
        if (allAvatarIds.Count == 0)
        {
            Debug.Log("No avatars available to show text through:" + pText);
            return;
        }

        int randomAvatarId = allAvatarIds[UnityEngine.Random.Range(0, allAvatarIds.Count)];
        AvatarView avatarView = _avatarAreaManager.GetAvatarView(randomAvatarId);
        avatarView.Say(pText);
    }

}
