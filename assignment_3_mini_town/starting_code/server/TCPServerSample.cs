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
    private Dictionary<TcpClient, ServerAvatar> _clientAvatarData = new Dictionary<TcpClient, ServerAvatar>();
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

            int posx = (int)(Math.Cos((new Random().Next(0, 180) * Math.PI) / 180)) * new Random().Next(0, 18);
            int posz = (int)(Math.Sin((new Random().Next(0, 180) * Math.PI) / 180)) * new Random().Next(0, 18);
            _clientAvatarData.Add(client, new ServerAvatar(indexAvatar, new Random().Next(0, 100), posx, 0, posz));

            Console.WriteLine("Accepted new client.");
            //Add a new character
            AvatarHandler newAvatarList = new AvatarHandler();
            newAvatarList.avatars = _clientAvatarData.Values.ToList<ServerAvatar>();

            foreach (TcpClient sendClient in _clients)
            {
                sendObject(sendClient, newAvatarList);
            }
        }
    }



    private void processExistingClients()
    {
        foreach (TcpClient client in _clients)
        {
            if (client.Available == 0) continue;

            //just send back anything we got
            StreamUtil.Write(client.GetStream(), StreamUtil.Read(client.GetStream()));
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

