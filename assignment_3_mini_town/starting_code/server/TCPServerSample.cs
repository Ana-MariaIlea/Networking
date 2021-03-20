using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;
using System.Linq;


/**
 * This class implements a simple tcp echo server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServerSample
{
    public static void Main(string[] args)
    {
        TCPServerSample server = new TCPServerSample();
        server.run();
    }

    private TcpListener _listener;
    private List<TcpClient> _clients = new List<TcpClient>();
    private List<TcpClient> _faultyClients = new List<TcpClient>();
    private List<TcpClient> _whisperClients = new List<TcpClient>();
    private Dictionary<TcpClient, ServerAvatar> _clientAvatarData = new Dictionary<TcpClient, ServerAvatar>();
    private List<MessageToSend> _clientMessageData = new List<MessageToSend>();
    private List<AvatarPosition> _newPositionRequests = new List<AvatarPosition>();
    private List<AvatarSkin> _newSkinRequests = new List<AvatarSkin>();
    private MessageToSend whisperMessage = null;
    private int indexAvatar = 1;

    private void run()
    {
        Console.WriteLine("Server started on port 55555");

        _listener = new TcpListener(IPAddress.Any, 55555);
        _listener.Start();

        while (true)
        {
            processNewClients();
            processExistingClients();
            RemoveFaultyClients();
            //Although technically not required, now that we are no longer blocking, 
            //it is good to cut your CPU some slack
            Thread.Sleep(100);
        }
    }

    private void processNewClients()
    {
        while (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();
            _clients.Add(client);
            Console.WriteLine("Accepted new client.");
            AddNewAvatarToClient(client);
            ActivateRingRequest ringActivation = new ActivateRingRequest();
            ringActivation.Id = indexAvatar - 1;
            sendObject(client, ringActivation);
        }
    }

    private void AddNewAvatarToClient(TcpClient client)
    {
        Random rand = new Random();
        double randomAngle = (rand.Next(0, 180) * Math.PI) / 180;
        int randomDistance = rand.Next(1, 18);
        //Make random position
        int posx = (int)(Math.Cos(randomAngle) * randomDistance);
        int posz = (int)(Math.Sin(randomAngle) * randomDistance);
        Console.WriteLine("Avatar added at " + posx + " " + 0 + " " + posz);
        Console.WriteLine("Random angle is " + randomAngle + " Random distance is" + randomDistance);
        //Make new skin

        int skinId = rand.Next(0, 100) % 4;
        //Add new avatar
        _clientAvatarData.Add(client, new ServerAvatar(indexAvatar, skinId, posx, 0, posz));
        indexAvatar++;

        SendAvatarListToClients();

    }

    private void SendAvatarListToClients()
    {
        //Send avatars to all clients
        AvatarHandler newAvatarList = new AvatarHandler();
        newAvatarList.avatars = _clientAvatarData.Values.ToList<ServerAvatar>();

        foreach (TcpClient sendClient in _clients)
        {
            sendObject(sendClient, newAvatarList);
        }
    }

    private void processExistingClients()
    {
        foreach (TcpClient client in _clients)
        {
            if (client.Available == 0) continue;

            //just send back anything we got
            // StreamUtil.Write(client.GetStream(), StreamUtil.Read(client.GetStream()));
            byte[] inBytes = StreamUtil.Read(client.GetStream());
            Packet inPacket = new Packet(inBytes);
            ISerializable inObject = inPacket.ReadObject();
            Console.WriteLine("Received:" + inObject);
            if (inObject is SimpleMessage) { handleMessage(client, inObject as SimpleMessage); }
            if (inObject is MoveRequest) { handlePositionChange(client, inObject as MoveRequest); }
        }

        if (_clientMessageData.Count > 0)
        {
            sendMessages();
        }
        if (_newPositionRequests.Count > 0)
        {
            UpdatePositions();
        }
        if (_newSkinRequests.Count > 0)
        {
            updateSkin();
        }
    }

    private void updateSkin()
    {
        Console.WriteLine("Send new skins");
        SetNewSkinResponse response = new SetNewSkinResponse();
        response.newSkins = _newSkinRequests;
        foreach (TcpClient sendClient in _clients)
        {
            sendObject(sendClient, response);
        }
        _newSkinRequests = new List<AvatarSkin>();
    }

    private void Whisper()
    {
        MessageResponses responses = new MessageResponses();
        List<MessageToSend> m = new List<MessageToSend>();
        m.Add(whisperMessage);
        responses.messages = m;
        foreach (TcpClient sendClient in _whisperClients)
        {
            sendObject(sendClient, responses);
        }
        _whisperClients = new List<TcpClient>();
    }
    private void UpdatePositions()
    {
        MoveResponse response = new MoveResponse();
        response.positions = _newPositionRequests;
        foreach (TcpClient sendClient in _clients)
        {
            sendObject(sendClient, response);
        }

        _newPositionRequests = new List<AvatarPosition>();
    }
    private void handlePositionChange(TcpClient pClient, MoveRequest pMessage)
    {
        Console.WriteLine("Update pos to " + pMessage.position.x + " " + pMessage.position.y + " " + pMessage.position.z + " by key " + _clientAvatarData[pClient].Id);
        AvatarPosition newPos = new AvatarPosition(pMessage.position.x, pMessage.position.y, pMessage.position.z, _clientAvatarData[pClient].Id);
        _newPositionRequests.Add(newPos);
        _clientAvatarData[pClient].ChangePosition(pMessage.position);
    }

    private void sendMessages()
    {
        MessageResponses response = new MessageResponses();
        response.messages = _clientMessageData;
        foreach (TcpClient sendClient in _clients)
        {
            sendObject(sendClient, response);
        }

        _clientMessageData = new List<MessageToSend>();
    }
    private void handleMessage(TcpClient pClient, SimpleMessage pMessage)
    {
        MessageToSend message = new MessageToSend(pMessage.text.text, _clientAvatarData[pClient].Id);
        if (message.IsMessageCommand())
        {
            if (message.GetCommand() == "/whisper")
            {
                handleWhisper(pClient, message);
            }
            if (message.GetCommand() == "/setskin")
            {
                int newSkin = -1;
                do
                {
                    Random rand = new Random();
                    newSkin = rand.Next(0, 100);
                    newSkin = newSkin % 4;
                } while (newSkin == _clientAvatarData[pClient].skinId);

                Console.WriteLine("add skin");
                _clientAvatarData[pClient].skinId = newSkin;
                _newSkinRequests.Add(new AvatarSkin(newSkin, _clientAvatarData[pClient].Id));
            }

        }
        else
        {
            _clientMessageData.Add(message);
            Console.WriteLine("message added to list");
        }
    }

    private void handleWhisper(TcpClient pClient, MessageToSend message)
    {
        whisperMessage = new MessageToSend(message.GetRestOfMessage(), message.sender);
        if (whisperMessage.text != null)
        {
            _whisperClients.Add(pClient);
            foreach (var client in _clients)
            {

                if (client != pClient)
                {
                    double distance = Math.Sqrt(Math.Pow(_clientAvatarData[client].posX - _clientAvatarData[pClient].posX, 2) +
                        Math.Pow(_clientAvatarData[client].posY - _clientAvatarData[pClient].posY, 2) +
                        Math.Pow(_clientAvatarData[client].posZ - _clientAvatarData[pClient].posZ, 2));
                    Console.WriteLine(distance);
                    if (distance <= 2)
                    {
                        _whisperClients.Add(client);

                        Console.WriteLine("message added to list");
                    }
                }
            }

            Whisper();
        }
    }

    private void sendObject(TcpClient pClient, ISerializable pOutObject)
    {
        try
        {
            Console.WriteLine("Sending:" + pOutObject);
            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);
            StreamUtil.Write(pClient.GetStream(), outPacket.GetBytes());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            CatchFaultyClients();
        }
    }

    private void RemoveFaultyClients()
    {
        if (_faultyClients.Count > 0)
        {
            foreach (var client in _faultyClients)
            {
                Console.WriteLine("Removed client: " + _clientAvatarData[client]);
                if (_clientAvatarData.ContainsKey(client))
                    _clientAvatarData.Remove(client);
                if (_clients.Contains(client))
                    _clients.Remove(client);
                client.Close();
            }

            _faultyClients = new List<TcpClient>();
            SendAvatarListToClients();

        }
    }

    private void CatchFaultyClients()
    {
        foreach (TcpClient client in _clients)
        {
            if (client.Connected == false)
            {
                Console.WriteLine("Faulty client directly: " + _clientAvatarData[client]);
                if (_faultyClients.Contains(client) == false)
                    _faultyClients.Add(client);
            }
        }

    }
}

